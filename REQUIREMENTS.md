# Pracovní dokument: Návrh systému „Firemní evidence faktur“  

## 1. Účel dokumentu  
Tento dokument slouží jako výchozí analýza požadavků na jednoduchý systém pro evidenci firemních faktur. Cílem je upřesnit rozsah funkcí, uživatele a očekávání před zahájením vývoje nebo další specifikace.

---

## 2. Zainteresované strany (Stakeholders)

- **Vedení firmy (management)**  
  - Zajímá je přehled o finančních tocích – kolik faktur je zaplaceno a kolik nezaplaceno.  
  - Potřebují rychlé reporty pro rozhodování.  
  - Chtějí možnost filtrovat faktury podle data, stavu platby nebo obchodního partnera.

- **Účetní / ekonomické oddělení**  
  - Jsou každodenními uživateli systému.  
  - Vkládají nové faktury, kontrolují jejich stav a exportují data pro účetnictví.  

- **Regulátor (Finanční úřad, obchodní inspekce)**  
  - Jsou nepřímo zainteresovaní – firma musí vést evidenci podle zákona.  
  - Systém musí umožňovat trvalou archivaci faktur (např. 10 let).  
  - Musí být splněny požadavky GDPR a daňové legislativy.  
  - V případě kontroly musí být možné vyexportovat všechny záznamy.

---

## 3. Základní požadavky na systém

- **Evidovat dva typy faktur**  
  - Přijaté faktury (od dodavatelů)  
  - Vydané faktury (pro zákazníky)

- **Správa faktur (CRUD operace)**  
  - Vytvoření nové faktury  
  - Zobrazení seznamu všech faktur  
  - Zobrazení detailu jedné faktury  
  - Úprava existující faktury  
  - Mazání faktury (např. při chybě při zadání)

- **Vyhledávání a filtrování**  
  - Filtrování podle data vystavení nebo splatnosti  
  - Filtrování podle stavu platby (zaplaceno / nezaplaceno)  
  - Filtrování podle názvu dodavatele nebo odběratele  
  - Filtrování podle čísla faktury  
  - (Poznámka: DPH sazby a pokročilé filtry lze řešit v budoucnu)

- **Bezpečnost a přístup**  
  - Přihlašování uživatelů
  - Role-based přístup:  
    - **Účetní**: může vytvářet, číst a upravovat faktury  
    - **Manažer**: může číst a filtrovat, nemůže mazat  
    - **Administrátor**: plná správa systému a uživatelů  
  - (Poznámka: Pro minimální verzi lze autentizaci a role zpočátku vynechat – k diskusi)

---

## 4. Scénáře použití (Use Cases)

- **Scénář 1: Účetní vloží novou fakturu**  
  Účetní zadá základní údaje o faktuře – číslo, datum vystavení, název partnera, celkovou částku a typ (přijatá/vydaná). Faktura se uloží do systému.

- **Scénář 2: Uživatel si zobrazí seznam faktur**  
  Účetní, manažer nebo auditor si může zobrazit všechny faktury nebo použít filtry (např. „nezaplacené přijaté faktury za poslední měsíc“).

- **Scénář 3: Účetní upraví existující fakturu**  
  V případě chyby (např. špatná částka nebo název firmy) účetní najde fakturu a opraví její údaje.


---

## 5. Otevřené otázky / Body k upřesnění

- Je přijatelné vynechat autentizaci v první verzi (např. pro lokální použití v malé firmě)?
- Stačí ukládat data do jednoduchého souboru (např. JSON), nebo je vyžadována databáze?
- Je nutné hned evidovat stav platby (zaplaceno / nezaplaceno), nebo postačí jen základní evidence faktur?
- Má být systém webovou aplikací, REST API nebo desktopovým nástrojem?

---

## 6. Další kroky (návrh)

- Upřesnit rozsah minimální verze (MVP) se zadavatelem nebo učitelem.
- Rozhodnout o technologickém řešení (např. Python + Flask, Node.js + Express, atd.).
- Případně vytvořit jednoduchý funkční prototyp pro demonstraci základních funkcí.


