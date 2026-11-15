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
| Ore conventionale/săpt. = Σ(ore × coeficient) / nr. săptămâni al semestrului | `WorkloadService.CalculateAsync` |
| Titular (`Head`): încadrare între minimul și maximul normei funcției; o repartizare care depășește maximul este refuzată | `WorkloadService` |
| Cumul (`InternalMultiJob` / `ExternalMultiJob`): limita = suma orelor contractelor active în anul universitar; fără contract activ nu se repartizează ore | `WorkloadService` |
| Contract: perioadă validă, ore > 0, număr unic, angajator de bază obligatoriu la cumul extern | `WorkloadController.CreateContract` |
| Stat de funcții: `Draft → Submitted → DepartmentApproved → FacultyApproved → Approved` (sau `Rejected` cu comentariu, apoi `Draft`); la depunere nimeni nu poate avea depășiri; la aprobare finală repartizările devin `Approved` | `StaffingPlanService` |

**Normele (`PositionNorms`) și coeficienții (`ConversionCoefficients`) nu sunt hardcodate**: se introduc în baza de date pentru fiecare an universitar, conform legii / regulamentului universității. Fără ele, calculul raportează `NotConfigured` / avertismente.

## Endpoint-uri noi

- `GET/POST /api/position-norms`, `GET/POST /api/conversion-coefficients`
- `GET /api/professors/{id}/workload?academicYear=2026-2027&semester=1`
- `GET /api/departments/{id}/workload?academicYear=`
- `GET /api/persons/{id}/contracts`, `POST /api/employments/{id}/contracts`, `POST /api/contracts/{id}/status`, `GET /api/contracts/expiring?days=30`
- `GET/POST /api/persons/{id}/documents`, `POST /api/documents/{id}/review`
- `POST /api/staffing-plans`, `GET /api/staffing-plans/{id}`, `POST /api/staffing-plans/{id}/transition`

`TeachingAssignment` are acum `AcademicYear` (obligatoriu, ex. `2026-2027`) și semestrul 1 sau 2.
