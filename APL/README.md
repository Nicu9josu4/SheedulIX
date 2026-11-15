# Academic Management (APL) — Managementul sarcinii didactice

API .NET 10 + PostgreSQL pentru:

1. **Calculul și repartizarea normelor didactice** — ore de curs, seminar și laborator pe cadru didactic, cu verificarea încadrării în limitele configurate.
2. **Evidența cadrelor didactice asociate (cumul)** — angajări multiple, contracte pe perioadă determinată, colectarea documentelor, aprobarea statelor de funcții.

## Pornire

```bash
docker compose -f ../postgres-compose.yml up -d   # PostgreSQL pe localhost:5432
dotnet ef database update                         # creează baza APL din migrări
dotnet run
```

## Reguli implementate

| Regulă | Unde |
| --- | --- |
| Cursul nu se împarte în subgrupe; profesorul trebuie să fie calificat pe disciplină și limbă; disciplina să fie în curriculumul grupei | `AcademicService.AssignProfessorAsync` |
| Subgrupe generate automat (2) dacă grupa are > 26 studenți | `AcademicService.SynchronizeSubgroupsAsync` |
| Ore conventionale pe an = Σ(ore × coeficient) pe ambele semestre | `WorkloadService.CalculateAsync` |
| Titular (`Head`): încadrare între minimul și maximul normei funcției; o repartizare care depășește maximul este refuzată | `WorkloadService` |
| Cumul (`InternalMultiJob` / `ExternalMultiJob`): limita = suma orelor contractelor active în anul universitar; fără contract activ nu se repartizează ore | `WorkloadService` |
| Contract: perioadă validă, ore > 0, număr unic, angajator de bază obligatoriu la cumul extern | `WorkloadController.CreateContract` |
| Stat de funcții: `Draft → Submitted → DepartmentApproved → FacultyApproved → Approved` (sau `Rejected` cu comentariu, apoi `Draft`); la depunere nimeni nu poate avea depășiri; la aprobare finală repartizările devin `Approved` | `StaffingPlanService` |

**Normele și coeficienții nu sunt hardcodate.** Ele trăiesc într-un **model de calcul pe an universitar** (`WorkloadModel`), cu ciclul `Draft → Active → Archived`:
- în `Draft` se completează normele anuale pe funcții (ore conventionale pe an: normă de bază, minim, maxim) și coeficienții pe tip de activitate;
- `Active` blochează modelul (se cere câte un coeficient pentru fiecare tip de activitate și cel puțin o normă); calculul avertizează dacă folosește un model încă `Draft`;
- pentru anul următor se **copiază** modelul anului precedent (`POST /api/workload-models/{id}/clone`) și se ajustează.
Fără model pentru un an, calculul raportează `NotConfigured` și avertismente.

## Endpoint-uri noi

- `GET/POST /api/workload-models`, `GET /api/workload-models/{id}`, `POST .../clone`, `.../norms`, `.../coefficients`, `.../activate`, `.../archive`
- `GET /api/professors/{id}/workload?academicYear=2026-2027`
- `GET /api/departments/{id}/workload?academicYear=`
- `GET /api/persons/{id}/contracts`, `POST /api/employments/{id}/contracts`, `POST /api/contracts/{id}/status`, `GET /api/contracts/expiring?days=30`
- `GET/POST /api/persons/{id}/documents`, `POST /api/documents/{id}/review`
- `POST /api/staffing-plans`, `GET /api/staffing-plans/{id}`, `POST /api/staffing-plans/{id}/transition`

`TeachingAssignment` are acum `AcademicYear` (obligatoriu, ex. `2026-2027`) și semestrul 1 sau 2.

## Import din Excel

Meniul **Import date** din interfață încarcă `UTM_sablon_import.xlsx` (șablon descărcabil, `wwwroot/templates/`). Sunt importate foile `Departamente`, `Cadre_didactice` și `Discipline`.

- `POST /api/import/preview` verifică fișierul fără să salveze și întoarce raportul pe foi și rânduri (noi / actualizate / neschimbate / erori).
- `POST /api/import/commit` salvează doar dacă nu există nicio eroare (totul sau nimic, într-o tranzacție).
- Idempotent: aceeași cheie (cod departament, email sau nume+prenume, cod+denumire disciplină) actualizează, nu dublează.
- Celelalte foi din șablon (planuri, grupe, calificări, model de calcul, contracte, repartizări) nu sunt încă importate.

## Disponibilitate (doleanțe de timp)

Meniul **Disponibilitate** permite fiecărui cadru didactic (completat de șeful de departament sau de profesor) să declare, pe an universitar:
- o **grilă săptămânală** zi × pereche cu stările *liber / nu pot / aș evita / prefer* (grila de perechi e în tabelul `TimeSlots`, valorile implicite trebuie confirmate cu orarul UTM);
- numărul **maxim de zile de predare** pe săptămână;
- **angajamente externe** (zi, interval, locație), care ocupă automat celulele grilei;
- observații.

Endpoint-uri: `GET /api/time-slots`, `GET|PUT /api/professors/{id}/availability?academicYear=`, `GET .../availability/feasibility?academicYear=`, `POST .../availability/parse`.

- **Interpretare din text liber** (`parse`): propune o grilă din fraze ca „nu pot joi, pot doar până la 12, prefer marți”. Nu salvează nimic; omul verifică grila și apoi salvează. Folosește **Claude** (`claude-opus-5-5`, rezultat JSON validat) dacă `ANTHROPIC_API_KEY` este setat în mediu sau `Anthropic:Enabled` este `true`; altfel, sau dacă apelul eșuează ori este refuzat, folosește **reguli simple** pentru formulările uzuale. Către Claude pleacă doar textul doleanței și grila de perechi, fără nume sau alte date personale.
- **Verificare de fezabilitate**: perechile necesare pe săptămână (ore pe semestru ÷ 14 săptămâni ÷ 2 ore pe pereche) față de perechile libere în cele mai bune zile permise.
- Aceste date vor fi folosite de generatorul de orar (etapa următoare); deocamdată se salvează și se verifică doar fezabilitatea sarcinii.

### Verificarea repartizărilor (disponibilitate + normă)

- `POST /api/teachingassignments/check` verifică o repartizare **fără să o salveze** și întoarce: erori (calificare, limbă, curriculum, curs pe subgrupe, depășirea normei), norma anuală rezultată, dacă orele încap în disponibilitatea declarată și avertismente. Erorile blochează salvarea; disponibilitatea insuficientă este doar avertisment (sunt dorințe, nu reguli).
- În ecranul **Sarcină didactică**, verificarea rulează în timp real pe măsură ce completezi formularul (cu an universitar și semestru).
- `GET /api/departments/{id}/feasibility?academicYear=` listează profesorii al căror volum de ore nu încape în disponibilitatea declarată.
- La depunerea statului de funcții, avertismentele de disponibilitate se adaugă în istoricul de aprobare (nu blochează depunerea).
