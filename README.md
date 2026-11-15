# SchedulIX

Platformă web pentru **managementul sarcinii didactice** la nivel de facultate: repartizarea orelor pe cadre didactice cu verificarea normelor, evidența cadrelor în cumul, doleanțele de timp ale profesorilor și **generarea automată a orarului** (cu export Excel și PDF).

## Documentație

| Document | Pentru cine |
| --- | --- |
| [Ghid de utilizare](APL/wwwroot/docs/GHID_UTILIZARE.md) | Șefi de departament și personal: cum se lucrează, pas cu pas |
| [Documentație tehnică](APL/wwwroot/docs/DOCUMENTATIE_TEHNICA.md) | Dezvoltatori: arhitectură, model de date, reguli, API, limitări |
| [README-ul aplicației](APL/README.md) | Rezumat al modulelor și endpoint-urilor |

Aceleași documente sunt disponibile în aplicație, la meniul **Ajutor**.

## Pornire rapidă

```bash
docker compose up -d            # PostgreSQL
dotnet tool install --global dotnet-ef
cd APL
dotnet ef database update       # creează baza din migrări
dotnet run                      # http://localhost:5291
```

Detalii și configurare: [Documentația tehnică, secțiunea 3](APL/wwwroot/docs/DOCUMENTATIE_TEHNICA.md).

## Structura repo-ului

- `APL/` — aplicația (API .NET 10 + frontend static)
- `import-templates/` — șablonul Excel de import
- `samples/` — planuri de învățământ în PDF (material de referință)
- `docker-compose.yml` — PostgreSQL pentru dezvoltare
