# \# Caiet de Sarcini (Technical Specifications)

# 

# \## System Overview

# 

# \*\*Nume Proiect:\*\* Engine Automatizat de Generare Orar (.NET Web API)

# 

# \*\*Tehnologii:\*\* .NET / C# Web API, Entity Framework Core, SQL Database (PostgreSQL/SQL Server), Export Engine (Excel/PDF)

# 

# Sistemul reprezintă o aplicație RESTful Web API destinată automatizării procesului de creare și gestionare a orarului academic, luând în considerare restricțiile de resurse (profesori, săli, grupe/subgrupe, tipuri de săptămâni) și preferințele/regulile specifice de predare.

# 

# \---

# 

# \## 1. Arhitectura \& Cerințe Funcționale

# 

# \### 1.1. Administrarea Sălilor și Resurselor

# 

# \* \*\*Categorii de Săli:\*\*

# \* Săli de curs (pentru serii/prelegeri).

# \* Laboratoare (cu opțiunea de dotare cu calculatoare/echipament specializat).

# \* Săli de seminar.

# 

# 

# \* \*\*Atribute de Bază:\*\*

# \* Capacitate (număr de locuri disponibile, ex. 10, 20, 100).

# \* Dotare tehnică (ex. `HasComputers`, `IsLab`).

# \* Bloc/Corp de învățământ.

# 

# 

# \* \*\*Gestionarea Disponibilității Sălilor (Calendarul Sălii):\*\*

# \* Definirea ferestrelor de disponibilitate (ex. sală oferită de un bloc vecin doar Lunea sau în intervalul 08:00 – 12:00).

# \* Starea sălii: Marcarea sălilor aflate în reparație/temporar indisponibile.

# \* Interfață API pentru vizualizarea calendarului de ocupare al fiecărei săli.

# 

# 

# 

# \### 1.2. Structura Academică (Grupe, Subgrupe, Serii)

# 

# \* \*\*Serii de Studiu:\*\* Capacitatea de a grupa mai multe grupe într-o serie pentru orele comune de curs (prelegeri).

# \* \*\*Dividere în Subgrupe:\*\* Posibilitatea ca o grupă să fie împărțită în două sau mai multe subgrupe (pentru lucrări de laborator/seminare).

# \* \*\*Frecvență \& Paritate Săptămânală:\*\*

# \* Alternanță săptămânală: Pară / Impară (ex. o subgrupă face laborator în săptămâna 1, cealaltă subgrupă în săptămâna 2).

# \* Suport pentru forme de învățământ: Cu frecvență (Zi) și Frecvență Redusă.

# 

# 

# 

# \### 1.3. Preferințe și Restricții Profesori

# 

# \* \*\*Preferințe Orare:\*\*

# \* Interval preferat de predare (ex. un profesor poate doar dimineața, altul doar după-amiază).

# 

# 

# \* \*\*Alocare Specifică de Săli:\*\*

# \* Posibilitatea de a lega un obiect/profesor de un cabinet specific (ex. \*Lucrările de laborator la Circuite Integrate\* se desfășoară obligatoriu în \*Sala 201\*).

# 

# 

# \* \*\*Restricții Didactice:\*\*

# \* Maximum \*\*3 cursuri/prelegeri cu același profesor\*\* pentru aceeași grupă în aceeași zi.

# 

# 

# 

# \---

# 

# \## 2. Reguli \& Algoritm de Generare Orar

# 

# | Categorie | Regula de Baza / Restricție |

# | --- | --- |

# | \*\*Număr maxim perechi\*\* | Studenții pot avea maximum \*\*4-5 perechi pe zi\*\*. |

# | \*\*Consecutivitate Cursuri\*\* | Nu se permit 3 cursuri consecutiv. Sunt permise combinații precum \*\*2 cursuri + 2 laboratoare\*\*. |

# | \*\*Distribuția Pachetului de Ore\*\* | Structura standard per disciplină: 2 cursuri, 2 seminare, 2 laboratoare. |

# | \*\*Separarea Seminarelor\*\* | Seminarul nu se recomandă a fi pus în aceeași zi cu cursul la aceeași disciplină (pentru a oferi timp de asimilare a materiei), deși este tehnic posibil. |

# | \*\*Grila Orară Fixă\*\* | Ore clare de început/sfârșit pentru fiecare pereche (1, 2, 3, 4, 5) și pauza de masă stabilită. |

# | \*\*Periodicitate\*\* | Săptămână Pară / Săptămână Impară. |

# 

# \---

# 

# \## 3. Export \& Raportare

# 

# \* \*\*Export Excel (`.xlsx`):\*\* Generarea orarului complet defalcat pe grupe, profesori și săli.

# \* \*\*Export PDF (`.pdf`):\*\* Format gata de tipar pentru afișare la avizier sau distribuție.

# \* \*\*Verificare Ocupare:\*\* Export/Interogare orar specific pentru o anumită sală.

# 

# \---

# 

# \## 4. Modelul Conceptual de Date (Schema BD)

# 

# ```

# \[TipSala] 1---\* \[Sala] 1---\* \[DisponibilitateSala]

# &#x20;                 1

# &#x20;                 |

# &#x20;                 \*

# \[PreferinteProfesor] \*---1 \[Profesor]

# &#x20;                 |

# &#x20;                 \*

# &#x20;              \[Orar] \*---1 \[Disciplina]

# &#x20;                 |

# &#x20;                 \*

# &#x20;              \[Grupa / Subgrupa / Serie]

# 

# ```

# 

# \### 4.1. Tabela `TipuriSali` (RoomTypes)

# 

# ```sql

# CREATE TABLE TipuriSali (

# &#x20;   Id INT PRIMARY KEY IDENTITY(1,1),

# &#x20;   Denumire NVARCHAR(100) NOT NULL, -- Curs, Laborator, Seminar

# &#x20;   AreCalculatoare BIT NOT NULL DEFAULT 0

# );

# 

# ```

# 

# \### 4.2. Tabela `Sali` (Rooms)

# 

# ```sql

# CREATE TABLE Sali (

# &#x20;   Id INT PRIMARY KEY IDENTITY(1,1),

# &#x20;   NumarSala NVARCHAR(50) NOT NULL, -- ex: "201", "Bloc B-102"

# &#x20;   Capacitate INT NOT NULL,         -- Număr locuri (ex: 10, 20, 100)

# &#x20;   TipSalaId INT NOT NULL FOREIGN KEY REFERENCES TipuriSali(Id),

# &#x20;   EsteDisponibila BIT NOT NULL DEFAULT 1 -- Status reparație / indisponibilitate

# );

# 

# ```

# 

# \### 4.3. Tabela `DisponibilitateSali` (RoomAvailabilities)

# 

# ```sql

# CREATE TABLE DisponibilitateSali (

# &#x20;   Id INT PRIMARY KEY IDENTITY(1,1),

# &#x20;   SalaId INT NOT NULL FOREIGN KEY REFERENCES Sali(Id),

# &#x20;   ZiuaSaptamanii INT NOT NULL,     -- 1 = Luni, ..., 6 = Sâmbătă

# &#x20;   OraInceput TIME NOT NULL,        -- ex: 08:00

# &#x20;   OraSfarsit TIME NOT NULL         -- ex: 12:00

# );

# 

# ```

# 

# \### 4.4. Tabela `PreferinteProfesori` (TeacherPreferences)

# 

# ```sql

# CREATE TABLE PreferinteProfesori (

# &#x20;   Id INT PRIMARY KEY IDENTITY(1,1),

# &#x20;   ProfesorId INT NOT NULL,

# &#x20;   Disciplinald INT NULL,

# &#x20;   IntervalPreferatInceput TIME NULL, -- ex: 08:00 (doar dimineata)

# &#x20;   IntervalPreferatSfarsit TIME NULL,  -- ex: 14:00

# &#x20;   SalaObligatorieId INT NULL FOREIGN KEY REFERENCES Sali(Id) -- ex: Sala 201

# );

# 

# ```

# 

# \### 4.5. Tabela `GrilaOrare` (TimeSlots)

# 

# ```sql

# CREATE TABLE GrilaOrare (

# &#x20;   NumarPereche INT PRIMARY KEY, -- 1, 2, 3, 4, 5

# &#x20;   OraInceput TIME NOT NULL,

# &#x20;   OraSfarsit TIME NOT NULL,

# &#x20;   EstePauzaMasa BIT NOT NULL DEFAULT 0

# );

# 

# ```

# 

# \### 4.6. Tabela `Orar` (Schedules)

# 

# ```sql

# CREATE TABLE Orar (

# &#x20;   Id INT PRIMARY KEY IDENTITY(1,1),

# &#x20;   DisciplinaId INT NOT NULL,

# &#x20;   ProfesorId INT NOT NULL,

# &#x20;   SalaId INT NOT NULL FOREIGN KEY REFERENCES Sali(Id),

# &#x20;   GrupaId INT NULL,

# &#x20;   SubgrupaId INT NULL,

# &#x20;   SerieId INT NULL,

# &#x20;   ZiuaSaptamanii INT NOT NULL, -- 1-6

# &#x20;   NumarPereche INT NOT NULL FOREIGN KEY REFERENCES GrilaOrare(NumarPereche),

# &#x20;   TipSaptamana INT NOT NULL    -- 0 = Toate, 1 = Pară, 2 = Impară

# );

# 

# ```

# 

# \---

# 

# \## 5. Endpoints REST API (Propunere Arhitecturală)

# 

# \### `POST /api/schedule/generate`

# 

# \* \*\*Descriere:\*\* Declanșează algoritmul automat de generare a orarului în baza regulilor definite.

# \* \*\*Input:\*\* `GenerateScheduleCommand` (An școlar, Semestru, Formă învățământ).

# 

# \### `GET /api/rooms/{id}/calendar`

# 

# \* \*\*Descriere:\*\* Returnează calendarul complet de ocupare și disponibilitate al unei săli.

# 

# \### `GET /api/schedule/export/excel`

# 

# \* \*\*Descriere:\*\* Exportă orarul curent în format `.xlsx`.

# 

# \### `GET /api/schedule/export/pdf`

# 

# \* \*\*Descriere:\*\* Exportă orarul curent în format `.pdf`.

# 

# \---

