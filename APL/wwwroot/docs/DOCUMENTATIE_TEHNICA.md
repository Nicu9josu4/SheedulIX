# SchedulIX / APL — Documentație tehnică

> Platformă web pentru **managementul sarcinii didactice** al unei facultăți: repartizarea orelor pe cadre didactice cu verificarea normelor, evidența cadrelor în cumul, doleanțele de timp ale profesorilor și **generarea automată a orarului**.
> Acest document descrie arhitectura, modelul de date, regulile de business, algoritmii și API-ul. Pentru utilizare, vezi [Ghidul de utilizare](GHID_UTILIZARE.md).

---

## 1. Prezentare generală

| Aspect | Valoare |
| --- | --- |
| Proiect | `APL/AcademicManagement.csproj` (namespace `AcademicManagement`) |
| Platformă | .NET 10, ASP.NET Core Web API (controllere) |
| Bază de date | PostgreSQL 16, acces prin Entity Framework Core 10 + Npgsql |
| Frontend | O singură pagină statică (`wwwroot/index.html`) cu Alpine.js și Tailwind CSS încărcate de pe CDN; nu are pas de build |
| Documentație API | Swagger UI (`/swagger`, doar în mediul Development) |
| Ramura de lucru | `alp` |

Modulele aplicației:

1. **Resurse**: departamente, cadre didactice și angajările lor (titular / cumul intern / cumul extern), discipline, programe de studii, curriculumuri, grupe și subgrupe.
2. **Sarcina didactică**: repartizări (profesor + disciplină + grupă + tip de activitate + ore), norme anuale pe funcție, contracte și documente pentru cumul, stat de funcții cu flux de aprobare.
3. **Disponibilitate**: grila săptămânală a fiecărui profesor, număr maxim de zile, angajamente externe, verificare de fezabilitate, interpretare din text liber (Claude sau reguli simple).
4. **Orar**: generare automată cu un solver de constrângeri, gestionarea sălilor, vizualizare și export Excel/PDF.
5. **Import din Excel** pentru departamente, cadre didactice și discipline.

## 2. Arhitectură

```
 Browser (Alpine.js + Tailwind)  ──HTTP/JSON──▶  Controllere (api/*)
                                                       │
                                                       ▼
                                              Servicii (reguli de business)
                      ┌────────────────────┬───────────┼───────────────┬───────────────────┐
                      ▼                    ▼           ▼               ▼                   ▼
              AcademicService      WorkloadService  AvailabilityService  TimetableService   ImportService
              (atribuiri, reguli)  (norme, ore conv.) (grilă, fezabilitate) (OR-Tools CP-SAT)  (ClosedXML)
                      │                    │           │               │                   │
                      └────────────────────┴───────────┴───────┬───────┴───────────────────┘
                                                                ▼
                                                   AppDbContext (EF Core) ──▶ PostgreSQL
```

- Straturi: **controller → serviciu → `AppDbContext`**. Controllerele sunt subțiri (validare de intrare, mapare erori în HTTP 400/404/409); regulile sunt în servicii.
- Erorile de business sunt excepții (`InvalidOperationException`, `KeyNotFoundException`, `ArgumentException`) transformate în răspunsuri `{ "error": "..." }`.
- Fișierele statice (frontend, documentația, șablonul Excel) sunt servite din `wwwroot` prin `UseFileServer()`.
- Nu există autentificare sau autorizare (vezi secțiunea 11).

### Structura soluției

```
APL/
├── Controllers/        AcademicControllers, WorkloadController, AvailabilityController,
│                       TimetableController, ImportController, CatalogController
├── Services/           AcademicService, WorkloadService, WorkloadModelService, StaffingPlanService,
│                       AvailabilityService, AvailabilityParser, TimetableService,
│                       TimetableExportService, ImportService
├── Models/             Dictionary.cs (nucleul), Workload.cs, Availability.cs, Timetable.cs
├── Data/DbContext.cs   AppDbContext: mapări, indecși, constrângeri, date inițiale (TimeSlots)
├── Migrations/         migrări EF Core
├── Properties/         launchSettings.json
├── wwwroot/            index.html, js/app.js, templates/ (șablon Excel), docs/ (documentația)
└── appsettings*.json
import-templates/      șablonul Excel de import (copie de lucru)
samples/                PDF-uri cu planuri de învățământ (material de referință)
```

## 3. Rulare locală

**Cerințe:** SDK .NET 10, Docker (pentru PostgreSQL), instrumentul `dotnet-ef`.

```bash
# 1. PostgreSQL (din rădăcina repo-ului)
docker compose up -d

# 2. Instrumentul EF (o singură dată)
dotnet tool install --global dotnet-ef

# 3. Creează baza APL din migrări
cd APL
dotnet ef database update

# 4. Pornește aplicația
dotnet run
```

Aplicația ascultă pe `http://localhost:5291` (și `https://localhost:7290`), conform `Properties/launchSettings.json`. Swagger: `/swagger`.

**Configurare** (`appsettings.json` sau variabile de mediu):

| Cheie | Rol |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | Conexiunea PostgreSQL. Implicit: `Host=localhost;Database=APL;Username=postgres;Password=password` (doar pentru dezvoltare) |
| `ANTHROPIC_API_KEY` (variabilă de mediu) | Activează interpretarea cu Claude a doleanțelor scrise liber. Fără ea se folosesc reguli simple |
| `Anthropic:Enabled` | `true` activează Claude și când autentificarea se face prin profil (`ant auth login`) |

## 4. Model de date

Cheile primare sunt `Guid`, cu excepția `TimeSlot` (număr de pereche). Enum-urile din tabelele noi se salvează ca text; cele din nucleul inițial (`EmploymentType`, `ActivityType`, `AssignmentStatus`) se salvează ca număr.

### 4.1. Resurse (`Models/Dictionary.cs`)

| Entitate | Câmpuri principale | Relații |
| --- | --- | --- |
| `Person` | nume, email, telefon, titlu și grad academic, `Active` | 1—* `DepartmentEmployment`, 1—1 `ProfessorProfile` |
| `Department` | `Code`, `Name`, `Description` | 1—* `DepartmentEmployment` |
| `DepartmentEmployment` | `EmploymentType` (`Head`=titular, `InternalMultiJob`, `ExternalMultiJob`), `Position`, `TeachingLoad`, date | *—1 `Person`, *—1 `Department` |
| `ProfessorProfile` | — | *—* `Language` (`ProfessorLanguage`), *—* `Subject` (`ProfessorSubject`: **calificările**) |
| `Language` | `Code` (RO, RU, EN), `Name` | |
| `Subject` | `Code`, `Name`, `Credits`, `TotalHours` | |
| `StudyProgram` | `Code`, `Name`, `DegreeLevel`, `DurationYears` | 1—* `Curriculum`, 1—* `Group` |
| `Curriculum` | `Version` (ex. `2026-2027`, anul de intrare), `IsActive`, `CreatedAt` | *—1 `StudyProgram`, 1—* `CurriculumSubject` |
| `CurriculumSubject` | an de studiu, semestru (1/2), ore de curs, seminar, laborator | *—1 `Curriculum`, *—1 `Subject`, *—1 `Language` |
| `Group` | `Code` (ex. `CR-251`), `YearOfStudy`, `EntryYear`, `StudentCount` | *—1 `StudyProgram`, *—1 `Curriculum`, 1—* `Subgroup` |
| `Subgroup` | `Name` (ex. `CR-251-SG1`), `StudentCount` | *—1 `Group` |
| `TeachingAssignment` | profesor, disciplină, grupă, subgrupă (opțional), limbă, `ActivityType` (curs/seminar/laborator), `Hours` (pe semestru), `Semester` (1/2), `AcademicYear` (`2026-2027`), `Status` | |

`Hours` este numărul total de ore pe semestru. Constrângeri în baza de date: `Hours > 0`, `Semester ∈ {1,2}`.

### 4.2. Sarcină didactică și cumul (`Models/Workload.cs`)

| Entitate | Rol |
| --- | --- |
| `WorkloadModel` | Modelul de calcul al unui an universitar (unic pe an): `Draft → Active → Archived` |
| `PositionNorm` | Norma anuală pe funcție, în ore convenționale: normă de bază, minim, maxim. Unică pe (model, funcție) |
| `ConversionCoefficient` | Coeficientul de transformare a orelor fizice în ore convenționale, pe tip de activitate. Unic pe (model, tip) |
| `Contract` | Contract pe perioadă determinată / cumul: număr (unic), tip, stare, perioadă, `AnnualHours`, angajator de bază |
| `EmploymentDocument` | Document colectat pentru o persoană: tip, stare (lipsă / depus / validat / respins), fișier |
| `StaffingPlan` | Statul de funcții al unui departament pe an (unic pe departament și an) |
| `ApprovalStep` | Istoricul tranzițiilor fluxului de aprobare (cine, când, comentariu) |

### 4.3. Disponibilitate (`Models/Availability.cs`)

| Entitate | Rol |
| --- | --- |
| `TimeSlot` | Grila de perechi (număr, început, sfârșit, pauză de masă). Date inițiale: 5 intervale provizorii (8:00–9:30, 9:45–11:15, 11:30–13:00, 13:00–14:30 = pauză, 14:30–16:00) |
| `AvailabilityBlock` | O celulă marcată (profesor, an, zi 1–6, pereche) cu starea `Blocked` / `Avoid` / `Preferred`; celulele nemarcate sunt libere |
| `AvailabilityPreference` | Număr maxim de zile de predare și observații, pe (profesor, an) |
| `ExternalCommitment` | Interval ocupat în altă instituție: zi, început, sfârșit, locație |

### 4.4. Orar (`Models/Timetable.cs`)

| Entitate | Rol |
| --- | --- |
| `Room` | Sală: cod, corp, capacitate, tip (`Lecture` / `Seminar` / `Laboratory`), calculatoare, `Active` |
| `TimetableRun` | O generare (an, semestru, stare solver, perechi totale / plasate, penalizare, timp). La o nouă generare, rularea anterioară a aceluiași semestru se șterge |
| `TimetableEntry` | O pereche plasată: repartizare, zi, pereche, sală, paritate (`All` / `Odd` / `Even`) |
| `TimetableUnplaced` | Perechi care nu au încăput, cu motivul |

Migrări: `InitialCreate`, `CurriculumEntryYear`, `WorkloadModelAnnualHours`, `Availability`, `Timetable`, `CatalogLanguages` (indice unic pe codul limbii și limbile implicite).

## 5. Reguli de business

### 5.1. Curriculum după cohortă
O grupă care începe studiile în anul **X** primește curriculumul activ al programului cu versiunea **X–Y**; dacă nu există, primește **ultimul curriculum creat** al programului (`AcademicService.ResolveCurriculumAsync`). Anul de intrare se deduce din codul grupei (`CR-251` → 2025). Endpoint: `POST /api/groups/auto-create`; verificare: `GET /api/curriculums/resolve`.

### 5.2. Subgrupe
O grupă cu **mai mult de 26 de studenți** primește automat 2 subgrupe echilibrate (`SynchronizeSubgroupsAsync`).

### 5.3. Validarea unei repartizări (`AcademicService.ValidateAssignmentAsync`)
1. Orele sunt pozitive, anul universitar este valid (`AAAA-AAAA+1`), semestrul este 1 sau 2.
2. Cursul nu poate fi împărțit pe subgrupe.
3. Profesorul trebuie să fie **calificat** pe disciplină și să poată preda în limba aleasă.
4. Disciplina cu limba respectivă trebuie să existe în **curriculumul grupei**.
5. Sarcina totală a profesorului trebuie să rămână în **limitele normei** (5.4).

### 5.4. Norma didactică (`WorkloadService`)
- **Ore convenționale pe an** = Σ (ore × coeficient pe tip de activitate), pe ambele semestre. Coeficienții și normele vin din `WorkloadModel`-ul anului (nu sunt în cod).
- **Titular** (`Head`): trebuie să fie între minimul și maximul normei funcției sale; peste maxim, repartizarea este refuzată; sub minim, starea este „Deficit”.
- **Cumul**: limita este suma orelor `AnnualHours` ale contractelor **active** din anul universitar; fără contract activ nu se pot repartiza ore.
- Fără model, normă sau coeficient configurat, calculul întoarce avertismente („normă neconfigurată”) și folosește coeficientul 1.

### 5.5. Ciclul de viață al modelului de calcul (`WorkloadModelService`)
`Draft` (se completează normele și coeficienții) → `Active` (blocat; cere câte un coeficient pentru fiecare tip de activitate și cel puțin o normă) → `Archived`. Pentru anul următor se **copiază** modelul precedent (`clone`) și se ajustează.

### 5.6. Statul de funcții (`StaffingPlanService`)
```
Draft → Submitted → DepartmentApproved → FacultyApproved → Approved
              └──────────── Rejected (cu comentariu) → Draft
```
- Depunerea este refuzată dacă un profesor din departament depășește limitele normei.
- Avertismentele de disponibilitate (5.7) se adaugă în istoricul de aprobare; **nu** blochează depunerea.
- La aprobarea finală, repartizările departamentului din acel an devin `Approved`.

### 5.7. Disponibilitate și fezabilitate (`AvailabilityService`)
- Fezabilitatea compară **perechile necesare pe săptămână** = ore pe semestru ÷ 15 săptămâni ÷ 2 ore pe pereche, cu **perechile libere** (celule care nu sunt `Blocked` și nu sunt acoperite de un angajament extern), limitate la cele mai bune zile permise de `MaxTeachingDays`.
- Constantele `WeeksPerSemester = 15` și `HoursPerPair = 2` sunt ipoteze de confirmat.
- `POST /api/teachingassignments/check` rulează validările 5.3, calculează norma rezultată și fezabilitatea pentru o repartizare ipotetică, **fără să salveze**. Erorile blochează salvarea din interfață; disponibilitatea insuficientă este doar avertisment.

### 5.8. Interpretarea doleanțelor din text (`AvailabilityParser`)
- **Cu Claude** (`claude-opus-5-5`, SDK-ul oficial `Anthropic`, răspuns JSON validat de o schemă): dacă `ANTHROPIC_API_KEY` este setat sau `Anthropic:Enabled` este `true`. Către model pleacă doar textul doleanței și grila de perechi, fără nume sau alte date personale. Orice celulă invalidă din răspuns este ignorată cu avertisment.
- **Reguli simple** (rezervă, și când Claude nu e configurat sau refuză): zile ale săptămânii, „până la / după ora X”, „dimineața / după-amiaza”, „doar …”, „nu pot …”, „prefer …”, „o dată / două zile pe săptămână”. Ce nu se înțelege apare ca avertisment.
- Rezultatul este doar o **propunere**; nu se salvează până nu o confirmă omul pe grilă.

### 5.9. Generarea orarului (`TimetableService`, OR-Tools CP-SAT)

**Sesiuni.** Din orele pe semestru rezultă `P = ⌈ore/2⌉` perechi; cu 15 săptămâni se obțin perechi **săptămânale** (`P/15`) și, pentru rest, perechi **la două săptămâni** (săptămâni pare sau impare). Numărul total poate diferi de plan cu câteva perechi (rotunjire).

**Variabile.** Pentru fiecare sesiune și fiecare opțiune posibilă (zi, pereche, paritate, sală eligibilă) o variabilă booleană; plus o variabilă „neplasată” pe sesiune. Exact una este adevărată.

**Constrângeri obligatorii:**
- o resursă (profesor, unitate de studenți, sală) nu poate fi în două locuri în aceeași celulă; sesiunile pe săptămâni pare și impare nu se ciocnesc între ele;
- o activitate pe grupă ocupă grupa și toate subgrupele ei; una pe subgrupă ocupă doar subgrupa;
- celulele `Blocked` și cele acoperite de angajamente externe sunt excluse;
- sala: curs → sală de curs; seminar → seminar sau curs; laborator → laborator; capacitate ≥ numărul de studenți;
- numărul maxim de zile de predare al profesorului;
- două perechi consecutive ale aceluiași profesor nu pot fi în **clădiri diferite**.

**Obiectiv (minimizare):** `1000 × neplasate + 3 × celule „aș evita” + 2 × zile de predare + (−1) × celule „prefer”`.

**Rezolvare:** limită de timp configurabilă (implicit 20 s, maxim 300 s), 8 fire de execuție. Stări: `Optimal` sau `Feasible` (cea mai bună soluție găsită în timp). Peste 400.000 de variante problema este refuzată ca prea mare.

**Rezultat:** intrările se salvează într-o rulare nouă, care o înlocuiește pe cea anterioară a semestrului. Sesiunile care nu au încăput apar la „neplasate”, cu motiv (nicio sală potrivită, profesor fără celule disponibile, sau nu a rămas loc fără conflicte).

### 5.10. Import din Excel (`ImportService`)
- Șablon: `UTM_sablon_import.xlsx` (în `wwwroot/templates/`). Antetul este în rândul 2, datele de la rândul 4; coloanele se recunosc după numele antetului (fără ` *`).
- Foi importate acum: **Departamente**, **Cadre_didactice**, **Discipline**. Celelalte foi ale șablonului sunt doar definiții de format.
- `preview` și `commit` rulează aceeași logică într-o **tranzacție**; previzualizarea o anulează mereu, iar `commit` doar dacă nu există nicio eroare (totul sau nimic).
- **Idempotent**: cheile sunt codul departamentului; emailul (sau nume + prenume) pentru persoane; cod + denumire pentru discipline. Aceeași cheie actualizează, nu dublează.
- Limită: 5 MB, doar `.xlsx`.

### 5.11. Export (`TimetableExportService`)
Excel cu ClosedXML (foile Grupe, Profesori, Săli, Neplasate, Rezumat) și PDF cu QuestPDF (A4 pe orizontală, câte o pagină pe grupă / profesor / sală).

## 6. Referință API

Toate rutele au prefixul `/api`. Erorile de business: HTTP 400 `{ "error": "..." }`; resursă lipsă: 404; conflict: 409. Valorile enum din JSON sunt text pentru `AvailabilityState`, `RoomType`, `WeekParity`; numere pentru restul (`EmploymentType`: 0 titular, 1 cumul intern, 2 cumul extern; `ActivityType`: 0 curs, 1 seminar, 2 laborator).

### Resurse
| Metodă | Rută | Rol |
| --- | --- | --- |
| GET, POST | `/departments` | Listă / adăugare departament |
| PUT, DELETE | `/departments/{id}` | Editare / ștergere (refuzată dacă are angajări sau state) |
| GET | `/persons/search?q=` | Căutare persoane |
| POST | `/persons` | Persoană + angajare + profil de profesor |
| PUT | `/persons/{id}` | Editarea persoanei și a angajării din departamentul indicat |
| POST | `/persons/{id}/employments` | Angajare suplimentară (cumul) |
| GET | `/professors` | Profesori cu angajări și limbi |
| GET, POST | `/subjects` | Discipline |
| PUT, DELETE | `/subjects/{id}` | Editare / ștergere (refuzată dacă e în curriculum sau repartizări) |
| GET, POST | `/studyprograms` | Programe de studii |
| PUT, DELETE | `/studyprograms/{id}` | Editare / ștergere (refuzată dacă are curriculumuri sau grupe) |
| GET, POST | `/curriculums` | Curriculumuri (cu disciplinele lor la GET); `version` în formatul `2026-2027` |
| PUT, DELETE | `/curriculums/{id}` | Editare / ștergere (refuzată dacă are grupe) |
| POST | `/curriculums/{id}/subjects` | Adaugă o disciplină în curriculum (an, semestru, ore curs/seminar/laborator) |
| DELETE | `/curriculums/{id}/subjects/{entryId}` | Scoate disciplina din curriculum |
| GET, POST | `/languages` | Limbi de instruire (codul este unic) |
| GET | `/curriculums/resolve?studyProgramId=&entryYear=` | Ce curriculum primește o cohortă |
| GET, POST | `/groups` | Grupe cu subgrupe |
| PUT | `/groups/{id}` | Editare; curriculumul trebuie să aparțină programului; subgrupele se refac la trecerea peste 26 de studenți |
| POST | `/groups/auto-create` | Grupă creată din cod (program, curriculum, subgrupe automate) |
| POST | `/groups/{id}/subgroups/generate` | Regenerează subgrupele |
| GET | `/dashboard/stats` | Cifre pentru panou |

### Sarcină didactică
| Metodă | Rută | Rol |
| --- | --- | --- |
| POST | `/teachingassignments` | Salvează o repartizare (cu toate validările) |
| POST | `/teachingassignments/check` | Verifică fără a salva: erori, normă, fezabilitate |
| GET, PUT | `/professors/{id}/qualifications` | Disciplinele și limbile pe care le poate preda un profesor (PUT înlocuiește lista) |
| GET, POST | `/workload-models` | Modele de calcul pe an |
| GET | `/workload-models/{id}` | Model cu norme și coeficienți |
| POST | `/workload-models/{id}/clone` · `/norms` · `/coefficients` · `/activate` · `/archive` | Gestionarea modelului |
| GET | `/professors/{id}/workload?academicYear=` | Ore convenționale, normă, stare |
| GET | `/departments/{id}/workload?academicYear=` | Idem, pe departament |
| GET, POST | `/persons/{id}/contracts`, `/employments/{id}/contracts` | Contracte |
| GET | `/contracts/expiring?days=` | Contracte care expiră |
| POST | `/contracts/{id}/status` | Schimbă starea unui contract |
| GET, POST | `/persons/{id}/documents` | Documente |
| POST | `/documents/{id}/review` | Validare / respingere document |
| POST | `/staffing-plans` | Creează statul de funcții |
| GET | `/staffing-plans/{id}` | Stat cu istoric de aprobare |
| POST | `/staffing-plans/{id}/transition` | Schimbă starea (`toStatus`, `actor`, `comment`) |

### Disponibilitate
| Metodă | Rută | Rol |
| --- | --- | --- |
| GET | `/time-slots` | Grila de perechi |
| GET, PUT | `/professors/{id}/availability?academicYear=` | Citește / salvează grila, zilele maxime, angajamentele |
| GET | `/professors/{id}/availability/feasibility?academicYear=` | Încape sarcina în disponibilitate? |
| POST | `/professors/{id}/availability/parse` | Propune o grilă din text (nu salvează) |
| GET | `/departments/{id}/feasibility?academicYear=` | Profesorii al căror volum nu încape |

### Orar și export
| Metodă | Rută | Rol |
| --- | --- | --- |
| GET, POST | `/rooms` | Săli |
| POST | `/rooms/{id}/active` | Activ / indisponibil (corp JSON `true` / `false`) |
| DELETE | `/rooms/{id}` | Șterge (refuzat dacă apare în orar) |
| POST | `/timetable/generate` | `academicYear`, `semester`, `timeLimitSeconds` |
| GET | `/timetable/summary?academicYear=&semester=` | Rezumatul ultimei generări |
| GET | `/timetable?academicYear=&semester=&groupId=&professorId=&roomId=` | Intrările orarului |
| GET | `/timetable/export/excel?academicYear=&semester=` | Fișier `.xlsx` |
| GET | `/timetable/export/pdf?academicYear=&semester=&view=&id=` | Fișier `.pdf` (`view`: `group`, `professor`, `room`, `all`) |

### Import
| Metodă | Rută | Rol |
| --- | --- | --- |
| POST | `/import/preview` | `multipart/form-data`, câmpul `file`: validează fără a salva |
| POST | `/import/commit` | Salvează dacă nu există erori |

## 7. Interfața web

Un singur fișier `index.html` (Alpine.js) cu logica în `js/app.js`. Meniul: Dashboard, Departamente, Programe de Studii, Angajați / Profesori, Discipline, Curriculum, Grupe & Subgrupe, Sarcină Didactică, Import date, Orar, Calificări, Model de calcul, Disponibilitate, Ajutor. Toate ecranele sunt legate la API; formularele de adăugare și editare funcționează pentru departamente, programe, discipline, curriculumuri (cu disciplinele lor), persoane și grupe. Paginile `demo-tabler.html` și `demo-vue.html` sunt machete de stil, nelegate la restul aplicației.

## 8. Stare și limitări cunoscute

**Funcții fără ecran (se fac prin API / Swagger):** contracte, documente, stat de funcții și fluxul de aprobare.

**Ștergerea:** ecranele oferă adăugare și editare; ștergerea există în API (`DELETE`, refuzată dacă înregistrarea e folosită), dar nu are buton.

**Date inițiale:** limbile RO, RU, EN sunt create de migrarea `CatalogLanguages` (fără a dubla pe cele existente). Restul datelor de referință (programe, curriculumuri, calificări, modelul de calcul) se introduc din ecrane sau din Excel.

**Alte limitări:**
- fără autentificare și roluri (director de departament, decan, resurse umane);
- importul Excel acoperă 3 din cele 9 foi ale șablonului;
- orarul nu implementează încă: maximum 4–5 perechi pe zi, interzicerea a 3 cursuri consecutive, seminarul în altă zi decât cursul, cursuri comune pe serii, ferestre de disponibilitate ale sălilor, timpi de deplasare între clădiri;
- fără teste automate (verificările făcute au fost manuale și cu scripturi);
- grila de perechi, numărul de săptămâni pe semestru (15) și valorile normelor sunt ipoteze de confirmat cu UTM.

## 9. Securitate și date personale

- Aplicația **nu are autentificare**: oricine ajunge la ea poate citi și modifica datele. Nu o expune în afara unei rețele de încredere înainte de a adăuga autentificare.
- Parola implicită din `appsettings.json` este doar pentru dezvoltare; în producție se folosește un secret (variabilă de mediu / user-secrets), nu un fișier din repo.
- Către Claude se trimit doar textul doleanței și grila de perechi, fără nume sau emailuri. Dacă UTM interzice trimiterea de text către servicii externe, lasă `ANTHROPIC_API_KEY` nesetat: se folosesc doar regulile locale.
- Datele personale ale cadrelor didactice (nume, email, contracte, documente) sunt în baza de date locală; se aplică politicile instituției privind protecția datelor.
- Importul acceptă doar `.xlsx` de până la 5 MB.

## 10. Dependențe și licențe

| Pachet | Rol | Licență |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore, Npgsql | acces la PostgreSQL | MIT / PostgreSQL |
| Swashbuckle.AspNetCore | Swagger | MIT |
| ClosedXML | citire / scriere Excel | MIT |
| Google.OrTools | solver CP-SAT | Apache 2.0 |
| Anthropic (SDK oficial) | interpretarea doleanțelor cu Claude | MIT |
| **QuestPDF** | export PDF | **Community**: verifică dacă se aplică instituției (https://www.questpdf.com/license/) |
| Alpine.js, Tailwind CSS, Font Awesome | frontend (CDN) | MIT / MIT / CC BY 4.0 + SIL OFL |

## 11. Ghid pentru dezvoltatori

- **Migrare nouă:** `dotnet ef migrations add NumeMigrare` apoi `dotnet ef database update`. Versiunea pachetului `Microsoft.EntityFrameworkCore.Design` trebuie să fie aceeași cu a restului pachetelor EF.
- **Enum-uri în JSON:** pentru text în loc de număr, adaugă `[JsonConverter(typeof(JsonStringEnumConverter))]` pe enum (cum fac `AvailabilityState`, `RoomType`, `WeekParity`). Nu activa global conversia, fiindcă frontend-ul vechi se bazează pe numere.
- **Reguli noi de orar:** se adaugă în `TimetableService` ca restricții pe `CpModel` (obligatorii) sau termeni în obiectiv (moi).
- **Erori de business:** aruncă `InvalidOperationException` cu un mesaj în română; controllerul îl transformă în HTTP 400.
- **Atenție la `dotnet ef database drop`:** ignoră opțiunea `--connection` și șterge baza din `appsettings.json`. Pentru o bază temporară folosește `DROP DATABASE` din `psql`.
- **Teste:** recomandat un proiect xUnit pentru `WorkloadService`, `AvailabilityParser.ParseWithRules`, `TimetableService.Pattern` și, cu o bază de test, `ImportService`.
