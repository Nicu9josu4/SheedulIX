# Instrucțiuni de pornire — PostgreSQL, pgAdmin și aplicația

Aplicația (proiectul `APL`) are nevoie de o bază de date PostgreSQL. Baza, pgAdmin (interfața web pentru bază) și aplicația se pornesc în această ordine.

## 0. Ce trebuie instalat (o singură dată)

| Program | Pentru ce | Verificare |
| --- | --- | --- |
| **Git** | descărcarea proiectului | `git --version` |
| **.NET 10 SDK** | rularea aplicației | `dotnet --version` (trebuie să înceapă cu 10) |
| **Docker Desktop** | PostgreSQL și pgAdmin | `docker --version` |
| **Unelte EF Core** | crearea tabelelor | `dotnet ef --version` |

Dacă `dotnet ef` nu e găsit:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

Pornește **Docker Desktop** și așteaptă până scrie că rulează, înainte de pasul 2.

## 1. Descarcă proiectul

```bash
git clone <adresa-repo>
cd SchedulIX
git checkout alp
```

Toate comenzile de mai jos se rulează din folderul proiectului (`SchedulIX`, cel care conține `docker-compose.yml`), dacă nu scrie altceva.

## 2. Pornește PostgreSQL și pgAdmin

```bash
docker compose up -d
```

Verifică:

```bash
docker compose ps
```

Ambele servicii (`postgres` și `pgadmin`) trebuie să apară ca `running`.

| Serviciu | Adresă | Utilizator | Parolă |
| --- | --- | --- | --- |
| PostgreSQL | `localhost:5432`, baza `APL` | `postgres` | `password` |
| pgAdmin | http://localhost:5050 | `admin@admin.com` | `admin` |

Parolele sunt doar pentru dezvoltare. Dacă calculatorul e folosit de alte persoane, schimbă-le în `docker-compose.yml` și în `APL/appsettings.json`.

### Conectarea pgAdmin la baza de date (o singură dată)
1. Deschide http://localhost:5050 și intră cu `admin@admin.com` / `admin`.
2. Clic dreapta pe **Servers** → **Register** → **Server…**
3. Fila **General**: Name = `APL local`.
4. Fila **Connection**:
   - Host name/address: `postgres`
   - Port: `5432`
   - Maintenance database: `APL`
   - Username: `postgres`
   - Password: `password`
5. **Save**. Tabelele apar la *Databases → APL → Schemas → public → Tables*.

Dacă pgAdmin nu se conectează cu `postgres`, încearcă `host.docker.internal` în loc de `postgres`.

## 3. Creează tabelele în baza de date

```bash
cd APL
dotnet ef database update
```

Comanda aplică toate migrările din `APL/Migrations` și creează tabelele. Se rulează o dată, apoi după fiecare actualizare a proiectului care aduce migrări noi.

Dacă ai un backup al bazei (`apl.dump`) și vrei să-l folosești, **nu rula comanda de mai sus**. Restaurează backup-ul:

```bash
docker compose cp apl.dump postgres:/tmp/apl.dump
docker compose exec postgres pg_restore -U postgres -d APL --no-owner --clean --if-exists /tmp/apl.dump
```

## 4. Pornește aplicația

```bash
cd APL
dotnet run
```

Când vezi `Now listening on: http://localhost:5291`, deschide în browser:

| Ce | Adresă |
| --- | --- |
| Aplicația | http://localhost:5291 |
| Documentația API (Swagger) | http://localhost:5291/swagger |

Oprește aplicația cu `Ctrl + C`.

Dacă `dotnet run` alege alt port sau deschide `https://localhost:7290`, folosește adresa afișată în terminal. Prima dată cu HTTPS poate cere să aprobi certificatul de dezvoltare:

```bash
dotnet dev-certs https --trust
```

## 5. Opțional: interpretarea doleanțelor cu Claude

Funcția de interpretare a doleanțelor din text liber folosește Claude doar dacă există cheia API în mediu. Fără ea, aplicația folosește reguli simple și funcționează normal.

PowerShell, înainte de `dotnet run`:

```powershell
$env:ANTHROPIC_API_KEY = "cheia-ta"
```

Nu pune cheia în fișierele din repo.

## Folosirea de zi cu zi

| Vreau să… | Comandă |
| --- | --- |
| Pornesc baza și pgAdmin | `docker compose up -d` |
| Pornesc aplicația | `cd APL` apoi `dotnet run` |
| Opresc baza și pgAdmin (datele rămân) | `docker compose down` |
| Șterg baza și toate datele | `docker compose down -v` (**atenție: ireversibil**) |
| Fac backup | `docker compose exec postgres pg_dump -U postgres -d APL -Fc -f /tmp/apl.dump` apoi `docker compose cp postgres:/tmp/apl.dump ./apl.dump` |
| Văd erorile bazei | `docker compose logs postgres` |

## Probleme frecvente

| Mesaj / situație | Cauză și soluție |
| --- | --- |
| `port is already allocated` (5432 sau 5050) | Alt PostgreSQL sau alt container folosește portul. Oprește-l, sau schimbă în `docker-compose.yml` portul din stânga (ex. `"5433:5432"`) și pune același port în `APL/appsettings.json`. |
| `Cannot connect to the Docker daemon` | Docker Desktop nu e pornit. |
| `password authentication failed` | Parola din `appsettings.json` nu coincide cu cea din `docker-compose.yml`. Dacă ai schimbat parola după prima pornire, volumul păstrează parola veche: `docker compose down -v` și repornește (se pierd datele). |
| `database "APL" does not exist` | Baza nu a fost creată: rulează pasul 3, sau verifică numele `APL` în compose. |
| `relation "..." does not exist` | Tabelele lipsesc: rulează `dotnet ef database update` din folderul `APL`. |
| `dotnet ef` nu e recunoscut | Instalează uneltele EF (pasul 0) și redeschide terminalul. |
| Aplicația pornește, dar nu se vede nimic | Verifică adresa din terminal (`Now listening on`) și că baza rulează (`docker compose ps`). |
