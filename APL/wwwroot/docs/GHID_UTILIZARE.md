# SchedulIX — Ghid de utilizare

> Pentru **șefii de departament** și personalul care introduce datele. Ghidul urmează ordinea firească a muncii: pregătești datele, repartizezi orele, verifici disponibilitatea profesorilor, generezi orarul și îl exporți.
> Detalii tehnice: [Documentația tehnică](DOCUMENTATIE_TEHNICA.md).

---

## 1. Ce face platforma

SchedulIX te ajută să răspunzi la patru întrebări:

1. **Cine predă ce?** Repartizezi orele de curs, seminar și laborator pe cadre didactice, iar platforma verifică pe loc dacă respectă regulile și norma.
2. **Încape în norma legală?** Calculează orele convenționale ale fiecărui profesor și le compară cu minimul și maximul funcției lui.
3. **Când poate preda fiecare?** Profesorii își declară zilele și orele în care nu pot (sau preferă) să predea.
4. **Cum arată orarul?** Generează automat orarul, ținând cont de profesori, grupe, săli și paritatea săptămânilor, apoi îl exportă în Excel sau PDF.

## 2. Accesarea platformei

Deschide în browser adresa pe care ți-a dat-o administratorul (pentru o instalare locală: `http://localhost:5291`). Meniul din stânga conține:

| Meniu | La ce folosește |
| --- | --- |
| **Dashboard** | Cifre generale: angajați, titulari / cumul, grupe, ore repartizate |
| **Departamente** | Lista și adăugarea departamentelor |
| **Programe de Studii** | Programele de studii (adăugare și editare) |
| **Angajați / Profesori** | Cadrele didactice și angajările lor |
| **Discipline** | Catalogul disciplinelor |
| **Curriculum** | Planurile de învățământ pe an de intrare, cu disciplinele lor |
| **Grupe & Subgrupe** | Grupele de studenți și subgrupele lor |
| **Sarcină Didactică** | Repartizarea orelor, cu verificare în timp real |
| **Import date** | Încărcarea datelor dintr-un fișier Excel |
| **Calificări** | Ce discipline și în ce limbi poate preda fiecare profesor |
| **Model de calcul** | Normele și coeficienții anului universitar |
| **Orar** | Generarea, vizualizarea și exportul orarului |
| **Săli** | Configurarea sălilor (cod, corp, tip, capacitate, stare) |
| **Disponibilitate** | Zilele și orele în care poate preda fiecare profesor |

## 3. Fluxul recomandat, pas cu pas

```
 1 Pregătire date → 2 Disponibilitate → 3 Repartizare ore → 4 Orar → 5 Export
 (Excel/ecrane)     (profesorii)         (cu verificare)     (generare)  (Excel/PDF)
```

| Pas | Ecran | Ce faci |
| --- | --- | --- |
| 1 | **Import date** | Încarci departamentele, cadrele didactice și disciplinele din Excel |
| 1 | **Grupe & Subgrupe** | Adaugi grupele (subgrupele se creează automat peste 26 de studenți) |
| 2 | **Disponibilitate** | Marchezi zilele și orele fiecărui profesor |
| 3 | **Sarcină Didactică** | Repartizezi orele; platforma verifică regulile și norma |
| 4 | **Săli** | Adaugi sălile (corp, capacitate, tip) |
| 5 | **Orar** | Generezi orarul |
| 6 | **Orar** | Exporți în Excel sau PDF |

> **Înainte de prima repartizare** pregătește, în această ordine: programele de studii și curriculumurile lor (secțiunea 3.1), calificările profesorilor (secțiunea 3.2) și modelul de calcul al anului (secțiunea 3.3). Limbile RO, RU și EN există deja.

### 3.1. Programe de studii, curriculumuri și grupe
1. **Programe de Studii**: adaugi programul (cod, nume, nivel, durată).
2. **Discipline**: adaugi disciplinele (sau le imporți din Excel).
3. **Curriculum**: creezi un curriculum pentru program, cu **versiunea = anul de intrare** (de exemplu `2026-2027`), apoi adaugi disciplinele lui, cu limba, anul de studiu, semestrul (1 sau 2) și orele de curs, seminar și laborator.
4. **Grupe & Subgrupe**: creezi grupa alegând programul, curriculumul și limba. Peste 26 de studenți se creează automat 2 subgrupe.

Un curriculum cu grupe asociate nu poate fi șters (îl poți dezactiva), iar un program cu curriculumuri sau grupe nu poate fi șters.

### 3.2. Calificările profesorilor
În **Calificări** alegi profesorul, bifezi **limbile** în care poate preda și **disciplinele** (cu căutare după nume sau cod; „Bifează cele afișate” selectează toate disciplinele filtrate), apoi apeși **Salvează calificările**. Fără calificare, o repartizare este refuzată.

### 3.3. Modelul de calcul al anului
În **Model de calcul**:
1. Scrii anul (de exemplu `2026-2027`) și apeși **Creează model gol**, sau alegi un model existent și apeși **Copiază modelul ales în acest an**.
2. În modelul **Ciornă** adaugi **normele** pe funcție (minim, normă de bază, maxim, în ore convenționale pe an; funcția se scrie ca în lista cadrelor, de exemplu „Lector universitar”) și **coeficienții** pentru curs, seminar și laborator.
3. Apeși **Activează modelul**. Activarea îl blochează și cere câte un coeficient pentru fiecare tip de activitate și cel puțin o normă. Valorile vin din regulamentul universității.

## 4. Importul datelor din Excel

1. Deschide **Import date**.
2. Apasă **Descarcă șablonul** și completează-l în Excel. Rândurile **galbene** sunt exemple: șterge-le și scrie datele tale. Coloanele cu antet **roșu și \*** sunt obligatorii; cele cu listă se aleg din meniul derulant.
3. Apasă **Alege fișier .xlsx**. Fișierul se verifică imediat, **fără să se salveze nimic**.
4. În tabelul de previzualizare vezi, pe fiecare foaie, câte rânduri sunt **noi**, **actualizate**, **neschimbate** și cu **erori**. Fiecare eroare spune foaia, rândul și motivul, de exemplu „Rând 9: Departamentul «NUEXISTA» nu există”.
5. Dacă nu sunt erori, apasă **Salvează în baza de date**. Dacă sunt erori, corectează-le în Excel și alege fișierul din nou. Butonul de salvare rămâne blocat până nu mai sunt erori.

**Ce se importă acum:** foile **Departamente**, **Cadre_didactice** și **Discipline**. Celelalte foi din șablon descriu formatul pentru viitor.

**Reguli utile:**
- Importul poate fi repetat: aceeași înregistrare (același cod de departament, același email, aceeași pereche cod + denumire de disciplină) se **actualizează**, nu se dublează.
- O persoană care lucrează în mai multe departamente apare pe **mai multe rânduri cu același email**.
- **Tip angajare**: *Titular*, *CumulIntern* sau *CumulExtern*.
- **Funcția** trebuie scrisă exact ca în modelul de calcul (de exemplu „Lector universitar”), pentru ca norma să se găsească.
- Fișierul trebuie să fie `.xlsx` de cel mult 5 MB.

## 5. Disponibilitatea profesorilor

În **Disponibilitate** alegi **profesorul** și **anul universitar** (de exemplu `2026-2027`).

### 5.1. Grila săptămânală
Rândurile sunt zilele (luni–sâmbătă), coloanele sunt perechile (cu ore). **Apasă pe o celulă** ca să-i schimbi starea, în această ordine:

| Culoare | Stare | Înseamnă |
| --- | --- | --- |
| alb | Liber | Poate preda |
| roșu ✕ | **Nu pot** | Nu poate preda deloc; orarul nu va pune nimic aici |
| galben ~ | **Aș evita** | Nu e de dorit; orarul evită, dar poate folosi dacă nu are alternativă |
| verde ✓ | **Prefer** | De preferat; orarul încearcă să folosească |
| gri ⛔ | **Ocupat extern** | Acoperit de un angajament în altă instituție (nu se schimbă manual) |

### 5.2. Număr maxim de zile
„Număr maxim de zile de predare pe săptămână”: de exemplu 1, dacă profesorul poate veni o singură dată pe săptămână.

### 5.3. Ocupat în altă facultate
La **Ocupat în altă facultate / instituție** alegi ziua, intervalul orar și locul (de exemplu „Facultatea X”) și apeși **Adaugă**. Celulele grilei acoperite de interval devin automat „Ocupat extern”.

### 5.4. Doleanța în cuvinte
În caseta **Scrie doleanța în cuvinte** poți scrie liber și apeși **Interpretează și completează grila**. Exemple care funcționează:

- „Nu pot joi. Pot doar până la ora 12.”
- „Vreau doar luni și miercuri după ora 13.”
- „Pot doar o dată pe săptămână.”
- „Sâmbăta nu pot deloc. Pot doar dimineața.”
- „Nu pot în ziua de joi, pot până la orele 12 și prefer marți.”

Aplicația propune o grilă; **verific-o** și corecteaz-o dacă e cazul, apoi apasă **Salvează disponibilitatea**. Nimic nu se salvează până nu apeși acest buton. Sub casetă vezi cu ce a fost interpretat textul (**Claude** sau **reguli simple**) și eventualele avertismente („Nu am înțeles: …”). Dacă o frază nu e înțeleasă, marchează manual pe grilă.

### 5.5. „Încape sarcina în disponibilitate?”
Panoul din dreapta compară orele deja repartizate profesorului cu perechile libere pe săptămână. Calculul: **ore pe semestru ÷ 15 săptămâni ÷ 2 ore pe pereche** = perechi necesare pe săptămână. Verde înseamnă că încape, roșu că disponibilitatea declarată este prea mică.

## 6. Repartizarea orelor

În **Sarcină Didactică** completezi:

| Câmp | Notă |
| --- | --- |
| Profesor, Disciplină, Grupă, Limbă | Alegi din liste |
| An universitar, Semestru | De exemplu `2026-2027`, semestrul 1 |
| Tip activitate | Curs, Seminar sau Laborator |
| Ore | **Total pe semestru** (de exemplu 30 pentru un curs de 15 perechi) |

### Verificarea în timp real
Pe măsură ce completezi, sub formular apare:

- **Roșu, blochează salvarea**: regula încălcată. De exemplu: profesorul nu este calificat pe disciplină, nu poate preda în limba aleasă, disciplina nu e în curriculumul grupei, cursul nu poate fi împărțit pe subgrupe, sau „Norma depășește limita maximă”.
- **Verde**: „Repartizarea respectă regulile.”
- **Bara normei**: orele convenționale pe an după repartizare, față de maxim, cu starea (în normă, sub minim, peste maxim, normă neconfigurată).
- **Galben, nu blochează**: avertismente, de exemplu „Nu încape: sunt necesare 7,1 perechi pe săptămână, dar disponibilitatea declarată oferă doar 4”. Disponibilitatea este o dorință, nu o regulă, deci decizi tu dacă continui.

Când nu mai sunt erori, apasă **Validează și Salvează Sarcina**.

### Ce sunt orele convenționale
Orele fizice se înmulțesc cu un coeficient pe tip de activitate (de exemplu cursul valorează mai mult decât laboratorul). Suma pe un an universitar se compară cu norma funcției profesorului: **minim – normă de bază – maxim**. Valorile le stabilește universitatea (modelul de calcul al anului).

**Cumul:** pentru cadrele în cumul limita este dată de contractele active; fără contract activ nu se pot repartiza ore.

## 7. Orarul

### 7.1. Sălile
În josul paginii **Orar**, la **Săli**, adaugi fiecare sală: **cod**, **corp**, **capacitate**, **tip** (curs / seminar / laborator) și, dacă e cazul, „calculatoare”. O sală aflată în reparație o marchezi **Indisponibilă** (click pe starea ei); nu o folosește generarea. O sală care apare deja în orar nu se șterge, doar se marchează indisponibilă.

### 7.2. Generarea
1. Alege **anul universitar** și **semestrul**.
2. Stabilește **Limita de timp** (secunde). 20 de secunde sunt de obicei suficiente; pentru probleme mari poți crește.
3. Apasă **Generează orarul**. Orarul anterior al aceluiași semestru este înlocuit.

Rezultatul arată:
- **Perechi plasate** (de exemplu 60 / 60);
- **Stare solver**: *Optim* (cea mai bună soluție) sau *Bun (timp epuizat)* (cea mai bună găsită în timpul dat);
- **Penalizare** (mai mică = mai bine) și **timpul de calcul**;
- **Perechi neplasate**, cu motivul.

### 7.3. Ce reguli respectă orarul
Nimic nu se suprapune: nici profesorul, nici grupa sau subgrupa, nici sala. Profesorul predă doar în celulele permise și respectă numărul maxim de zile. Sala este potrivită ca tip și capacitate. Perechile pe **săptămâni pare / impare** nu se ciocnesc între ele. Două perechi consecutive ale aceluiași profesor nu pot fi în **clădiri diferite**. Orarul evită celulele „aș evita” și încearcă să folosească „prefer”.

### 7.4. Motivele pentru perechi neplasate
| Motiv | Ce faci |
| --- | --- |
| nicio sală activă potrivită ca tip și capacitate | Adaugă sau activează o sală de tipul și capacității cerute |
| profesorul nu are celule disponibile (blocaje sau angajamente externe) | Discută cu profesorul disponibilitatea sau redu blocajele |
| nu a rămas loc liber fără conflicte | Crește limita de timp, adaugă săli, relaxează zile blocate sau repartizează altfel |

### 7.5. Vizualizare
Alege **Vizualizare după**: *Grupă*, *Profesor* sau *Sală*, apoi elementul dorit. În celulă vezi disciplina, grupa (și subgrupa), profesorul, sala cu corpul și, la perechile la două săptămâni, „pare” sau „impare”. Culorile: albastru curs, verde seminar, galben laborator.

### 7.6. Export
- **Export Excel**: un fișier cu foile Grupe, Profesori, Săli, Neplasate și Rezumat.
- **Export PDF**: pagini A4 pe orizontală, gata de tipar, câte una pe grupă / profesor / sală, după vizualizarea aleasă. Cu o grupă aleasă obții PDF-ul ei; cu „Toate” obții PDF-ul tuturor grupelor.

## 8. Cumul, contracte, state de funcții (prin API)

Aceste funcții există, dar **nu au încă ecran**. Se folosesc din **Swagger** (`/swagger`, administratorul îți dă adresa): deschizi ruta, apeși **Try it out**, completezi și **Execute**.

**Contract de cumul:** `POST /api/employments/{id}/contracts` cu, de exemplu:
```json
{ "number": "C-2026-014", "type": 1, "status": 1,
  "startDate": "2026-09-01", "endDate": "2027-08-31",
  "annualHours": 250, "baseEmployer": "Întreprinderea X" }
```
(`type`: 0 pe perioadă determinată, 1 asociat, 2 plata cu ora; `status`: 1 activ). Contractele care expiră curând: `GET /api/contracts/expiring?days=30`.

**Stat de funcții:** `POST /api/staffing-plans` (`departmentId`, `academicYear`), apoi `POST /api/staffing-plans/{id}/transition` cu `{ "toStatus": 1, "actor": "Nume", "comment": "..." }`. Fluxul: Ciornă (0) → Depus (1) → Aprobat de departament (2) → Aprobat de facultate (3) → Aprobat (4); Respins (5) cere comentariu. La depunere nu pot exista profesori peste normă; avertismentele de disponibilitate se văd în istoric.

**Calculul normei unui profesor:** `GET /api/professors/{id}/workload?academicYear=2026-2027`.

## 9. Glosar

| Termen | Sens |
| --- | --- |
| **Pereche** | Un interval fix de curs (de exemplu 8:00–9:30), echivalent cu 2 ore |
| **Normă didactică** | Numărul de ore convenționale pe an pe care îl are de predat un cadru, în funcție de funcție |
| **Ore convenționale** | Orele fizice înmulțite cu coeficientul tipului de activitate |
| **Titular** | Cadru cu funcția de bază la această instituție |
| **Cumul intern / extern** | Ore suplimentare la aceeași instituție / la alta, pe bază de contract |
| **Săptămână pară / impară** | O pereche care se ține o dată la două săptămâni |
| **Subgrupă** | Jumătate de grupă, pentru laborator (se creează automat peste 26 de studenți) |
| **Curriculum** | Planul de învățământ al unui program, pe an de intrare |
| **Cohortă** | Studenții care încep studiile în același an |

## 10. Întrebări frecvente și probleme

| Mesaj / situație | Cauză | Ce faci |
| --- | --- | --- |
| „Profesorul nu este calificat pentru această disciplină” | Nu are disciplina în lista de calificări | Adaug-o în **Calificări** (secțiunea 3.2) |
| „Profesorul nu este autorizat să predea în această limbă” | Limba nu e în lista profesorului | Bifeaz-o în **Calificări** |
| „Disciplina cu această limbă nu există în curriculumul grupei” | Disciplina nu e în planul grupei | Verifică grupa, limba și curriculumul |
| „Norma depășește limita maximă” | Suma orelor convenționale depășește maximul funcției | Redu orele sau repartizează altui profesor |
| „Nu există model de calcul pentru 2026-2027” / „normă neconfigurată” | Modelul anului nu e creat sau nu are funcția respectivă | Creează și activează modelul în **Model de calcul** (secțiunea 3.3) |
| „Nu există nicio sală activă” la generare | Nu ai adăugat săli | Adaugă săli în **Orar** |
| „Nu există repartizări pentru acest an și semestru” | Nimic repartizat în semestrul ales | Repartizează ore în **Sarcină Didactică** |
| Import: „Departamentul «…» nu există” | Cadrul trimite la un cod de departament inexistent | Adaugă departamentul în foaia **Departamente** |
| Orarul are multe perechi neplasate | Prea puține săli, prea multe blocaje sau limită de timp mică | Vezi tabelul din 7.4 |
| Interpretarea textului spune „Nu am înțeles” | Formularea nu e recunoscută de regulile simple | Marchează manual pe grilă |

## 11. Sfaturi de lucru

- Completează **disponibilitatea înainte** de a repartiza și de a genera orarul: scade numărul de perechi neplasate.
- Verifică în **Sarcină Didactică** avertismentele galbene: indică din timp profesorii care nu vor încăpea în orar.
- Generează orarul de mai multe ori dacă schimbi datele: fiecare generare înlocuiește pe cea precedentă a aceluiași semestru. Exportă înainte dacă vrei să păstrezi o variantă.
- Păstrează o copie a fișierului Excel de import folosit; îl poți reîncărca oricând fără să dublezi datele.

## 12. Ce nu are încă ecran

- **Contracte, documente și state de funcții** (secțiunea 8) se fac din Swagger.
- **Ștergerea** înregistrărilor nu are buton în ecrane; adăugarea și editarea funcționează.
- **Autentificarea și rolurile** (director de departament, decan, resurse umane) nu sunt implementate: oricine are adresa aplicației poate modifica datele.
