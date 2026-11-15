# Caiet de Sarcini (Technical Specifications)

## System Overview

**Nume Proiect:** SchedulIX — Platformă de Management al Sarcinii Didactice (.NET Web API)

**Tehnologii:** .NET / C# Web API, Entity Framework Core, PostgreSQL, Export Engine (Excel/PDF)

SchedulIX este o aplicație RESTful Web API care înlocuiește foile de calcul (Excel/Google Sheets) folosite de departamentele universitare pentru:

1. **Calculul și repartizarea normelor didactice** — distribuirea orelor de curs, seminar și laborator pe fiecare cadru didactic, cu verificarea încadrării în limitele legale.
2. **Evidența cadrelor didactice asociate (cumul)** — contracte pe perioadă determinată, colectarea documentelor și aprobarea statelor de funcții.

Generatorul de orar (săli, grila orară, grupe/subgrupe) rămâne un **modul secundar**: după ce sarcina didactică este stabilită și aprobată, orarul se generează pornind de la cine predă ce.

---

## 1. Module funcționale

### 1.1. Calculul și repartizarea normelor didactice

* **Plan de învățământ → necesar de ore.** Pentru fiecare disciplină se definesc orele de curs, seminar și laborator (`Discipline`).
* **Repartizare (`TeachingAssignment`).** O repartizare leagă un cadru didactic de o disciplină, un tip de activitate (curs/seminar/laborator), o formație de studiu (serie, grupă sau subgrupă), numărul de ore pe săptămână și numărul de săptămâni, într-un an universitar și semestru.
* **Ore convenționale.** Tipurile de activitate au coeficienți de conversie configurabili (`ConversionCoefficient`), pe an universitar și, opțional, pe formă de învățământ.
* **Norma cadrului didactic.** Fiecare funcție didactică (`AcademicPosition`: profesor, conferențiar, lector, asistent) are, pe an universitar, o normă de bază și limite minime/maxime (`PositionNorm`).
* **Calcul și validare.** Sistemul calculează orele convenționale repartizate, deficitul sau excedentul față de normă și orele peste normă, și semnalează încălcarea limitelor.
* **Parametri configurabili.** Valorile normelor și coeficienților **nu sunt scrise în cod**; se păstrează în baza de date, pentru a fi adaptate legislației sau regulamentului universității.

### 1.2. Evidența cadrelor didactice asociate (cumul)

* **Tip de angajare.** Fiecare cadru este titular, asociat (cumul) sau plata cu ora (`EmploymentType`).
* **Contracte pe perioadă determinată (`Contract`).** Număr, tip, perioadă, ore contractate pe săptămână, starea contractului; alerte la apropierea expirării.
* **Documente (`TeacherDocument`).** Colectarea documentelor necesare (CV, copie CI, diplome, adeverință de la angajatorul de bază etc.), cu stare: lipsă / depus / validat / respins.
* **Aprobarea statelor de funcții (`StaffingPlan` + `ApprovalStep`).** Statul de funcții al unui departament, pe an universitar, parcurge fluxul:

```
Draft → Submitted → DepartmentApproved → FacultyApproved → Approved
              ↘ Rejected (cu comentariu, revine la Draft)
```

Fiecare tranziție este înregistrată (cine, când, comentariu) ca `ApprovalStep`.

### 1.3. Modulul de orar (secundar)

* **Săli:** de curs, laborator, seminar; capacitate, dotări, bloc, ferestre de disponibilitate și stare (reparație).
* **Structură academică:** serii, grupe, subgrupe; alternanță pară/impară; forme de învățământ (zi / frecvență redusă).
* **Preferințe profesori:** interval preferat, sală obligatorie pentru o disciplină.
* **Reguli de generare:** max. 4–5 perechi/zi, fără 3 cursuri consecutiv, max. 3 cursuri ale aceluiași profesor pentru aceeași grupă pe zi, seminar de preferat în altă zi decât cursul.
* **Export:** Excel/PDF pe grupe, profesori și săli.

---

## 2. Model conceptual de date

```
[Department] 1---* [Teacher] *---1 [AcademicPosition] 1---* [PositionNorm]
                       |
        +--------------+----------------+
        |              |                |
        *              *                *
[TeachingAssignment] [Contract] 1---* [TeacherDocument]
   *---1 [Discipline]
   *---1 [StaffingPlan] 1---* [ApprovalStep]
   *---0..1 [Series | AcademicGroup | Subgroup]

[ConversionCoefficient]  (configurare: tip activitate -> coeficient)

Modul orar: [RoomType] 1---* [Room] 1---* [RoomAvailability]
            [Schedule] *---1 [Teacher | Discipline | Room | TimeSlot | Group ...]
```

### 2.1. Entități noi

| Entitate | Câmpuri principale |
| --- | --- |
| `Department` | Name, Code |
| `AcademicPosition` | Title, Code |
| `PositionNorm` | AcademicPositionId, AcademicYear, BaseNormHours, MinHours, MaxHours |
| `ConversionCoefficient` | AcademicYear, ActivityType, EducationFormId?, Coefficient |
| `TeachingAssignment` | TeacherId, DisciplineId, StaffingPlanId?, AcademicYear, Semester, ActivityType, SeriesId?/GroupId?/SubgroupId?, WeeklyHours, Weeks |
| `StaffingPlan` | DepartmentId, AcademicYear, Status, CreatedAt, UpdatedAt |
| `ApprovalStep` | StaffingPlanId, FromStatus, ToStatus, ActorName, Comment, CreatedAt |
| `Contract` | TeacherId, Number, Type, StartDate, EndDate, WeeklyHours, Status, BaseEmployer? |
| `TeacherDocument` | TeacherId, ContractId?, Type, FileName, StoragePath, Status, UploadedAt, ValidatedAt?, ValidatedBy? |

### 2.2. Entități extinse

* `Teacher`: + `DepartmentId?`, `AcademicPositionId?`, `EmploymentType`.

### 2.3. Entități existente (modulul de orar)

`RoomType`, `Room`, `RoomAvailability`, `TeacherPreference`, `Discipline`, `TimeSlot`, `AcademicGroup`, `Subgroup`, `Series`, `EducationForm`, `Schedule`.

---

## 3. Endpoints REST API

### Existente (orar)

* `POST /api/schedule/generate` — generează orarul.
* `GET /api/rooms/{id}/calendar` — calendarul unei săli.
* `GET /api/schedule/export/excel` și `/pdf` — exporturi.

### Propuse (sarcină didactică)

* `GET /api/teachers/{id}/workload?academicYear=` — sarcina, norma și abaterea unui cadru.
* `POST /api/assignments` / `PUT` / `DELETE` — repartizarea orelor.
* `GET /api/departments/{id}/staffing-plan?academicYear=` — statul de funcții.
* `POST /api/staffing-plans/{id}/submit` · `/approve` · `/reject` — fluxul de aprobare.
* `GET/POST /api/teachers/{id}/contracts` — contracte.
* `GET/POST /api/teachers/{id}/documents` — documente.
* `GET /api/contracts/expiring?days=` — contracte care expiră curând.
* `GET /api/staffing-plans/{id}/export/excel` și `/pdf` — export state de funcții.

---

## 4. Etape de implementare

1. ✅ Documentație actualizată și model de date extins (entități + `DbContext`).
2. ⬜ Migrare EF Core și date inițiale (funcții didactice, coeficienți, norme).
3. ⬜ `WorkloadCalculationService` și validările legale.
4. ⬜ Contracte, documente și fluxul de aprobare.
5. ⬜ Autentificare cu roluri (cadru didactic, director departament, decan, resurse umane).
6. ⬜ Legarea modulului de orar de repartizările aprobate.
