# Implementeringsprojekt (RAD2026)

Programmet understøtter tre forskellige kommandoer, opdelt pba. opgaverne i [beskrivelsen](https://github.com/BimmerBass/RAD2026/blob/main/Implementeringsprojekt.pdf). Programmet køres ved følgende kommando,
```powershell
dotnet run --project RadImplementationProject -- <command> [options]
```
**Bemærk:** ved at tilføje ```--property:Configuration=Release```  køres processen hurtigere.

## ```hash-functions``` (Del 1, opgave 1.c)

Sammenligner køretiden af Multiply-shift med Multiply-mod-Prime

```powershell
dotnet run --project RadImplementationProject -- hash-functions --stream-size <n> [--bit-width <l>]
```

| # | Krævet | Default-værdi | Beskrivelse |
| --- | --- | --- | --- |
| `--stream-size <n>` | Nej | `1UL << 24` | Antal elementer i strømmen. |
| `--bit-width <l>` | No | `20` | Antal unikke nøgler i strømmen. $l \in [1;64)$, samt $2^l \leq n$. |

For at reproducere rapporten, kør følgende kommando:

```powershell
dotnet run --project RadImplementationProject -- hash-functions
```

## ```quadratic-sums``` (Del 1, opgave 3)

Beregner eksakte *second-moment* værdier for forskellige $l$-værdier med hhv. Multiply-Shift og Multiply-mod-Prime og sammenligner deres køretid.

```powershell
dotnet run --project RadImplementationProject -- quadratic-sums [--stream-size <n>] [--l-samples <values>]
```

| # | Krævet | Default-værdi | Beskrivelse |
| --- | --- | --- | --- |
| `--stream-size <n>` | Nej | `1UL << 24` | Antal elementer i strømmen. |
| `--l-samples <values>` | Nej | `1, 2, 4, 8, 12, 16, 20` | Komma-separaret liste af $l$-værdier. Alle har $l_i \in [1;64)$, samt $2^{l_i} \leq n$ |

For at reproducere rapporten, kør følgende kommando:

```powershell
dotnet run --project RadImplementationProject -- quadratic-sums
```

## ```count-sketch``` (Del 2)

Udfører et eller flere CountSketch eksperimenter. Strømmen genereres en gang på baggrund af ```--stream-size``` og ```--bit-width```, og der køres et CountSketch eksperiment for hver $m$-værdi.

Resultaterne gemmes i en CSV-fil
```powershell
dotnet run --project RadImplementationProject -- count-sketch [--stream-size <n>] [--bit-width <l>] [--m-bit-widths <values>] --csv-path <path>
```
| # | Krævet | Default-værdi | Beskrivelse |
| --- | --- | --- | --- |
| `--stream-size <n>` | Nej | `1UL << 24` | Antal elementer i strømmen. |
| `--bit-width <l>` | Nej | `23` |  Antal unikke nøgler i strømmen. $l \in [1;64)$, samt $2^l \leq n$. |
| `--m-bit-widths <values>` | Nej | `23` | Komma-separaret liste af $m$-bit-antal. Antal counters i et CountSketch eksperiment vælges til $2^m$ |
| `--csv-path <path>` | Ja | | Fil som resultaterne skal gemmes i. |

### Del 2, opgave 7
```powershell
dotnet run --project RadImplementationProject -- count-sketch --csv-path results_opg7.csv
```

### Del 2, opgave 8
```powershell
dotnet run --project RadImplementationProject -- count-sketch --m-bit-widths 8,12,23 --csv-path results_opg8.csv
```

# Databehandling
For et generere plots og analysere dataen fra vores eksperimenter har vi benyttet Jupyter notebooks. Disse er ikke nødvendige for at køre forsøgene, men er med for at gøre analysen reproducérbar.

For at lave et nyt virtuelt miljø og installere afhængigheder, kør følgende kommandoer i projektets rod:
```powershell
python -m venv .env
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```
Ligeledes skal resultat-CSV'erne kopieres fra deres respektive mapper, ind i roden af projektet.