# Ladestander – Designdokument

---

# 1. Introduktion

Ladestander-projektet er udviklet som en moderne webapplikation til administration af opladningsdata for elbilbrugere i et bofællesskab med to fælles ladestandere og 20 husstande. Projektet erstatter en eksisterende løsning udviklet i Microsoft Access, som tidligere blev anvendt til håndtering af kunder, import af opladningsdata og beregning af fakturagrundlag.

Formålet med projektet er at udvikle en mere vedligeholdbar, sikker og testbar løsning baseret på moderne webteknologier og lagdelt arkitektur. Systemet er opbygget som en .NET-baseret webapplikation bestående af:

* et Blazor-baseret administrationsfrontend
* et REST-baseret API
* en Microsoft SQL Server-database

Projektet fokuserer særligt på:

* Software Quality
* testbarhed
* sikkerhedsdesign
* separation of concerns
* vedligeholdbar arkitektur
* korrekt håndtering af forretningsregler og fakturadata

Systemet er udviklet som en MVP-løsning (Minimum Viable Product), hvor fokus primært er rettet mod backend-funktionalitet, sikkerhed, testbarhed og korrekt databehandling frem for fuld produktionsmodenhed eller avanceret brugeroplevelse.

Løsningen understøtter blandt andet:

* administration af kunder
* administration af perioder
* import af ladeposter fra CSV-filer
* generering af fakturaer
* håndtering af fakturastatus
* rollebaseret administratoradgang

Ladeposter importeres fra CSV-filer eksporteret fra ChargerSync, som er ABB E-mobilitys administrations- og overvågningsplatform til konfiguration, styring og monitorering af Terra AC-ladestandere.

Projektets arkitektur er bevidst designet med fokus på:

* løs kobling
* dependency injection
* isolerbar forretningslogik
* testbarhed
* sikker håndtering af autentificering og autorisation

Designet understøtter dermed både systematisk testarbejde og videreudvikling af systemet.

Projektet fungerer samtidig som praktisk demonstrationsprojekt inden for:

* Secure Software Development
* Software Quality
* design for testability
* lagdelt systemarkitektur
* white-box testing
* moderne .NET-baseret backend-udvikling

---

# 2. Systemarkitektur

Ladestander-systemet er udviklet som en lagdelt webapplikation baseret på .NET 10 med fokus på separation of concerns, testbarhed og sikker håndtering af forretningslogik og opladningsdata.

Systemet består overordnet af tre hoveddele:

* et Blazor-baseret administrationsfrontend
* et REST-baseret backend-API
* en Microsoft SQL Server-database

Arkitekturen er designet med tydelig adskillelse mellem præsentationslag, forretningslogik og persistenslag for at reducere koblingen mellem systemets komponenter og understøtte vedligeholdbarhed og testbarhed.

![Ladestander architecture](images/architecture-diagram.png)

## Overordnet arkitektur

Frontendløsningen er udviklet som en Blazor Web App (`Ladestander.Web`), som fungerer som administrationsinterface for systemets administratorbrugere. Frontenden håndterer blandt andet:

* login
* administration af kunder
* administration af perioder
* import af ladeposter
* visning af fakturaer
* håndtering af fakturastatus

Frontenden kommunikerer med backend-systemet gennem HTTP-baserede API-kald til `Ladestander.Api`.

Backend-løsningen (`Ladestander.Api`) fungerer som systemets centrale forretnings- og integrationslag. API’et håndterer:

* autentificering og autorisation
* validering af requests
* forretningsregler
* fakturagenerering
* CSV-import
* databaseadgang
* fejl- og statushåndtering

Persistenslaget er implementeret med Microsoft SQL Server og Entity Framework Core, hvor databasen fungerer som centralt lagringspunkt for:

* kunder
* perioder
* ladeposter
* fakturaer
* administratorbrugere

## Lagdelt arkitektur

Backend-systemet er organiseret som en lagdelt arkitektur bestående af:

* controller-lag
* service-lag
* repository-lag
* persistenslag

### Controller-lag

Controller-laget fungerer som systemets HTTP-interface og håndterer:

* routing
* request/response-håndtering
* statuskoder
* inputvalidering
* autentificeringskrav

Controllerne indeholder bevidst minimal forretningslogik. Deres primære ansvar er at validere input og videresende requests til relevante services.

Denne adskillelse reducerer koblingen mellem HTTP-laget og systemets forretningslogik.

### Service-lag

Service-laget indeholder systemets centrale forretningslogik og fungerer som bindeled mellem controllers og repositories.

Eksempler på ansvar i service-laget:

* fakturagenerering
* håndtering af fakturastatus
* CSV-import
* beskyttelse mod dubletter
* autentificeringslogik
* validering af forretningsregler

Forretningslogikken er placeret i service-laget for at:

* understøtte testbarhed
* undgå logik i controllers
* gøre systemets regler lettere at vedligeholde
* understøtte white-box testing

### Repository-lag

Repository-laget fungerer som abstraktionslag mellem forretningslogik og databaseadgang.

Repositories håndterer blandt andet:

* queries
* persistens
* entity-opslag
* opdateringer
* databasefiltrering

Adgangen til repositories sker gennem interfaces som eksempelvis:

* `ICustomerRepository`
* `IInvoiceRepository`
* `IChargingSessionRepository`

Denne struktur understøtter dependency injection og gør det muligt at erstatte repositories med mocks under unit tests.

### Persistenslag

Persistenslaget er implementeret med Entity Framework Core og Microsoft SQL Server.

`AppDbContext` fungerer som centralt persistenslag og håndterer:

* entity mapping
* relationer
* constraints
* foreign keys
* databaseopdateringer

Databasen er designet med fokus på relationel integritet og datakonsistens. Der anvendes blandt andet:

* foreign key constraints
* unikke constraints
* indexes
* nullable relationer hvor relevant

## Dependency Injection og løs kobling

Systemet anvender dependency injection gennem hele backend-arkitekturen.

Services, repositories og sikkerhedskomponenter registreres centralt i `Program.cs` og injiceres via constructor injection.

Denne tilgang reducerer koblingen mellem systemets komponenter og understøtter:

* testbarhed
* udskiftelige implementationer
* løs kobling
* bedre vedligeholdbarhed

Dependency injection spiller samtidig en central rolle i projektets design for testability-strategi, fordi afhængigheder kan erstattes med mocks under unit tests.

## Arkitekturmæssige designmål

Arkitekturen er udviklet med fokus på følgende centrale designmål:

* tydelig separation of concerns
* høj testbarhed
* løs kobling mellem komponenter
* vedligeholdbarhed
* sikker håndtering af autentificering og autorisation
* understøttelse af Software Quality-principper
* understøttelse af Secure Software Development-principper

Disse designvalg danner grundlag for både systemets sikkerhedsdesign og teststrategi, som beskrives i senere afsnit af dokumentet.

---

# 3. Domænemodel og Forretningslogik

Systemets centrale domænemodel er udviklet med fokus på tydelig adskillelse mellem kunder, perioder, ladeposter og fakturaer. Domænemodellen danner grundlag for både systemets databasearkitektur, forretningslogik og teststrategi.

Domænet er bevidst designet omkring relativt simple og tydelige entiteter for at gøre:

* forretningsregler lettere at forstå
* persistens mere overskuelig
* testdesign mere kontrollerbart
* systemet lettere at vedligeholde

Systemets centrale domæneobjekter består af:

* kunder (`Customer`)
* perioder (`BillingPeriod`)
* ladeposter (`ChargingSession`)
* fakturaer (`Invoice`)
* administratorbrugere (`AdminUser`)

![Ladestander database design](images/database-design.png)

## Kunder (Customer)

`Customer` repræsenterer en elbilbruger i bofællesskabet.

En kunde indeholder blandt andet:

* navn
* adresseoplysninger
* RFID-nummer
* e-mailadresse
* aktiv/inaktiv-status

RFID-nummeret anvendes som centralt identifikationspunkt ved import af ladeposter fra ChargerSync.

Kunder fungerer samtidig som centralt relationelt omdrejningspunkt for:

* ladeposter
* fakturaer
* perioder

Systemet er designet til at understøtte fremtidig udvidelse med egentlige brugerlogins for kunder, men MVP-versionen understøtter kun administratoradgang.

## Perioder (BillingPeriod)

`BillingPeriod` repræsenterer en afgrænset faktureringsperiode.

En periode indeholder blandt andet:

* startdato
* slutdato
* gennemsnitlig pris pr. kWh
* status for lukket/åben periode

Perioder anvendes til:

* gruppering af ladeposter
* fakturagenerering
* låsning af historiske data

Når en periode markeres som lukket, forhindrer systemet:

* import af nye ladeposter
* ændringer af eksisterende fakturaer
* generering af nye fakturaer

Dette design reducerer risikoen for ændringer i historiske fakturadata.

## Ladeposter (ChargingSession)

`ChargingSession` repræsenterer en enkelt opladningssession importeret fra ChargerSync.

En ladepost indeholder blandt andet:

* starttidspunkt
* energiforbrug i kWh
* ladenavn
* brugerinformation fra importfilen
* relation til kunde
* relation til periode
* relation til faktura

Ladeposter importeres fra CSV-filer eksporteret fra ChargerSync, som er ABB E-mobilitys administrations- og overvågningsplatform til konfiguration, styring og monitorering af Terra AC-ladestandere.

Importflowet indeholder flere mekanismer til:

* validering
* kunde-matching
* beskyttelse mod dubletter
* håndtering af fejlscenarier

En ladepost kan eksistere uden relation til en faktura, fordi import og fakturagenerering foregår som separate workflows.

Dette understøttes gennem nullable relation mellem `ChargingSession` og `Invoice`.

## Fakturaer (Invoice)

`Invoice` repræsenterer en genereret faktura for en kunde i en bestemt periode.

En faktura indeholder blandt andet:

* kundenummer
* periode
* samlet energiforbrug
* samlet beløb
* fakturanummer
* status
* oprettelsestidspunkt

Systemet understøtter flere fakturastatusser:

* Draft
* Sent
* Paid

Statusflowet kontrolleres centralt i service-laget for at forhindre ugyldige statusændringer.

Eksempelvis:

* kan Paid-fakturaer ikke ændres
* kan Sent-fakturaer ikke ændres tilbage til Draft
* kan fakturaer ikke genereres for lukkede perioder

Fakturaer gemmer samtidig snapshot-data som:

* `TotalEnergyKWh`
* `TotalAmount`

Dette design sikrer, at historiske fakturaer ikke ændres, selv hvis:

* ladeposter ændres senere
* energipriser ændres
* periodedata opdateres

Snapshot-strategien blev valgt for at sikre historisk datakonsistens og stabil fakturering.

## Administratorbrugere (AdminUser)

`AdminUser` repræsenterer systemets administratorer.

Administratorbrugere anvendes til:

* login
* autentificering
* rollebaseret adgangskontrol

MVP-versionen understøtter kun administratoradgang og ikke egentlige kundeprofiler med login.

Administratorbrugere indeholder blandt andet:

* brugernavn
* password hash
* rolleinformation

Passwords gemmes aldrig i plaintext, men hashes med PBKDF2 gennem ASP.NET Cores `PasswordHasher`.

## Relationer mellem domæneobjekter

Domænemodellen er designet med tydelige relationer mellem centrale entiteter.

En kunde kan have:

* flere ladeposter
* flere fakturaer

En periode kan indeholde:

* flere ladeposter
* flere fakturaer

En faktura kan være relateret til:

* én kunde
* én periode
* flere ladeposter

Disse relationer håndhæves gennem foreign keys og relationel integritet i databasen.

## Centrale forretningsregler

Systemets forretningslogik håndhæves primært i service-laget og omfatter blandt andet følgende regler:

* der må kun eksistere én faktura pr. kunde pr. periode
* ladeposter må ikke importeres til lukkede perioder
* dublerede ladeposter må ikke oprettes
* betalte fakturaer må ikke ændres
* ugyldige statusovergange afvises
* ladeposter skal være relateret til gyldige kunder og perioder

Disse regler er centrale for både:

* datakonsistens
* korrekt fakturering
* systemets teststrategi
* white-box testing

Domænemodellen er dermed tæt koblet til både systemets Software Quality-overvejelser og sikker håndtering af forretningskritiske data.

---

# 4. API- og Backenddesign

Backend-løsningen er implementeret som et REST-baseret API udviklet i ASP.NET Core (.NET 10). API’et fungerer som systemets centrale integrations- og forretningslag og håndterer:

* autentificering
* validering
* forretningsregler
* persistens
* fejlbehandling
* kommunikation mellem frontend og database

API-designet er udviklet med fokus på:

* tydelig separation of concerns
* testbarhed
* vedligeholdbarhed
* konsistent request/response-struktur
* kontrolleret håndtering af fejlscenarier

## Controllers

Controller-laget fungerer som systemets HTTP-interface og håndterer:

* routing
* statuskoder
* request parsing
* autentificeringskrav
* inputvalidering
* videresendelse til services

Systemet indeholder blandt andet følgende controllers:

* `AuthController`
* `CustomersController`
* `BillingPeriodsController`
* `ChargingSessionsController`
* `InvoicesController`

Controllerne er bevidst holdt relativt tynde og indeholder minimal forretningslogik. Formålet er at:

* reducere kompleksitet i HTTP-laget
* gøre controllerne lettere at teste
* centralisere forretningsregler i service-laget

Denne struktur understøtter samtidig white-box testing af forretningslogikken uden direkte afhængighed til HTTP-laget.

## DTO’er

API’et anvender DTO’er (Data Transfer Objects) som kontrakt mellem klient og backend.

DTO’erne anvendes til:

* request-data
* response-data
* inputvalidering
* begrænsning af eksponeret data
* stabilisering af API-kontrakter

Eksempler på DTO’er:

* `CreateCustomerRequestDto`
* `UpdateCustomerRequestDto`
* `GenerateInvoiceRequestDto`
* `ChargingSessionImportRequestDto`
* `LoginRequestDto`
* `LoginResponseDto`

Brugen af DTO’er reducerer koblingen mellem:

* domænemodellen
* persistenslaget
* API-kontrakter

Dette gør det lettere at ændre interne entities uden at bryde klientkommunikation.

## Inputvalidering

Inputvalidering håndteres primært gennem:

* DataAnnotations
* model validation
* defensive checks i service-laget

Eksempler på validering:

* obligatoriske felter
* gyldige datoer
* positive numeriske værdier
* gyldige statusovergange
* gyldige relationer mellem entiteter

Validering udføres både:

* på API-niveau
* i forretningslogikken

Denne dobbelte valideringsstrategi reducerer risikoen for ugyldige eller inkonsistente data.

## Services

Service-laget indeholder størstedelen af systemets forretningslogik og fungerer som centralt workflow-lag mellem controllers og repositories.

Eksempler på services:

* `CustomerService`
* `BillingPeriodService`
* `ChargingSessionService`
* `ChargingSessionCsvImportService`
* `InvoiceService`
* `AuthService`

Services håndterer blandt andet:

* fakturagenerering
* statusflows
* importlogik
* autentificeringslogik
* beskyttelse mod dubletter
* forretningsvalidering

Forretningsregler placeres bevidst i service-laget for at:

* understøtte testbarhed
* reducere controller-kompleksitet
* centralisere regler
* gøre white-box testing lettere

## CSV-importflow

CSV-import håndteres gennem `ChargingSessionCsvImportService`.

Importflowet:

1. læser CSV-filen
2. parser rækker
3. matcher kunder
4. validerer data
5. kontrollerer dubletter
6. opretter ladeposter
7. registrerer importresultater

Importlogikken er designet til at håndtere:

* ugyldige perioder
* manglende kunder
* dublerede ladeposter
* ugyldige CSV-rækker
* continue-paths ved fejl

Formålet er at sikre robust og kontrolleret import af ladeposter uden at stoppe hele importprocessen ved enkelte fejl.

## Repositories

Repositories fungerer som abstraktionslag mellem service-lag og persistenslag.

Eksempler:

* `CustomerRepository`
* `InvoiceRepository`
* `ChargingSessionRepository`
* `BillingPeriodRepository`
* `AdminUserRepository`

Repositories håndterer:

* queries
* filtrering
* persistens
* entity-opslag
* databaseopdateringer

Adgangen sker gennem interfaces for at understøtte:

* dependency injection
* mocks under unit tests
* løs kobling mellem lag

## Mapping-strategi

Systemet anvender manuel mapping mellem:

* DTO’er
* entities
* response-modeller

Mappingen håndteres gennem dedikerede mapper-klasser som:

* `CustomerMapper`
* `InvoiceMapper`
* `ChargingSessionMapper`

Denne tilgang blev valgt frem for automatisk mapping for at:

* gøre dataflow mere eksplicit
* reducere skjult logik
* forbedre debugging
* øge kontrol over transformationslogik

## Error Handling og API-fejl

API’et anvender kontrolleret fejlbehandling gennem:

* globale exception handlers
* valideringsfejl
* defensive checks
* kontrollerede statuskoder

Systemet returnerer blandt andet:

* `400 Bad Request`
* `401 Unauthorized`
* `403 Forbidden`
* `404 Not Found`
* `409 Conflict`
* `429 Too Many Requests`

Fejlmeddelelser returneres som strukturerede responses for at:

* forbedre frontendhåndtering
* understøtte debugging
* gøre API’et mere konsistent

Valideringsfejl returneres eksempelvis som standardiserede fejlobjekter med information om:

* fejltype
* felt
* fejlbeskrivelse

## Global Exception Handling

Systemet anvender centraliseret exception handling gennem globale exception handlers.

Denne strategi blev valgt for at:

* undgå duplikeret fejlbehandling i controllers
* sikre ensartede API-fejl
* reducere kompleksitet i controller-laget

Særlige fejltyper som:

* gateway-fejl
* SOAP-fejl
* valideringsfejl
* databasefejl

kan dermed håndteres konsistent på tværs af systemet.

## API-design og statuskoder

API’et er designet med fokus på:

* tydelige endpoints
* konsistente HTTP-metoder
* forudsigelige statuskoder
* kontrollerede fejlresponses

Eksempler:

* `POST` anvendes til oprettelse
* `GET` anvendes til læsning
* `PUT` anvendes til opdateringer
* `PATCH` anvendes til delvise statusændringer

Eksempelvis anvendes:

* `PATCH` til fakturastatus
* `PATCH` til reopening af perioder

for tydeligt at signalere, at der er tale om begrænsede tilstandsændringer frem for fulde entity-opdateringer.

API-designet understøtter dermed både:

* vedligeholdbarhed
* testbarhed
* sikkerhed
* tydelig klientkommunikation

---

# 5. Sikkerhedsdesign

Sikkerhedsdesignet i Ladestander-projektet er udviklet med fokus på Secure Software Development og secure by design-principper. Systemet håndterer forbrugs- og fakturadata for elbilbrugere, og selv om MVP-versionen primært er rettet mod administratorbrug, er det stadig vigtigt, at adgang, input, sessions og følsomme data håndteres kontrolleret.

Sikkerhedsdesignet fokuserer særligt på:

* autentificering
* autorisation
* rollebaseret adgangskontrol
* sikker sessionhåndtering
* inputvalidering
* password hashing
* rate limiting
* beskyttelse af følsomme data
* separation of concerns

![Authentication flow](images/authentication-flow.png)

## Autentificering og autorisation

Systemet anvender JWT-baseret autentificering i API-laget. Når en administrator logger ind, valideres brugernavn og password gennem `AuthService`. Hvis credentials er gyldige, genererer `JwtService` et JWT-token med relevante claims, herunder brugernavn og rolle.

API’et anvender rollebaseret adgangskontrol, hvor beskyttede endpoints kræver en gyldig JWT og administratorrollen.

Denne struktur sikrer, at:

* kun autentificerede administratorbrugere kan tilgå beskyttede endpoints
* adgangskontrol håndhæves centralt i API-laget
* autorisation ikke overlades til frontendlaget
* roller kan udvides senere, hvis systemet får flere brugertyper

MVP-versionen anvender kun administratorrollen, men designet kan udvides med eksempelvis kunde-login og mere detaljerede rollemodeller.

## JWT og cookie-baseret session

Systemet anvender en hybrid sikkerhedsmodel mellem Blazor-frontenden og API’et.

API’et bruger JWT til autentificering, mens Blazor-frontenden anvender en cookie-baseret session. Når administratoren logger ind via frontenden, sendes credentials til API’et. API’et returnerer et JWT-token, som derefter gemmes server-side i brugerens ClaimsPrincipal i webapplikationen.

Browseren modtager ikke JWT-tokenet direkte. I stedet modtager browseren en secure HttpOnly cookie.

Dette design blev valgt for at undgå lagring af JWT i:

* localStorage
* sessionStorage
* browsertilgængelig JavaScript-kontekst

Dermed reduceres risikoen for, at tokenet eksponeres ved XSS-angreb.

Blazor-webapplikationen anvender efterfølgende JWT-tokenet server-side, når den kalder API’et.

## Cookie-konfiguration

Cookie-sessionen er konfigureret med sikkerhedsattributter som:

* HttpOnly
* Secure
* SameSite=Strict
* fast udløbstid
* deaktiveret sliding expiration

`HttpOnly` forhindrer adgang til cookien via JavaScript, mens `Secure` sikrer, at cookien kun sendes over HTTPS. `SameSite=Strict` reducerer risikoen for cross-site request forgery ved at begrænse, hvornår cookien sendes med cross-site requests.

Den faste udløbstid og deaktiveret sliding expiration gør sessionens levetid mere forudsigelig og reducerer risikoen for utilsigtet lange sessioner.

## Rollebaseret adgangskontrol

API’et anvender rollebaseret adgangskontrol til at beskytte administrative endpoints. Administratorrollen anvendes til at afgrænse adgang til funktioner som:

* kundeadministration
* periodehåndtering
* import af ladeposter
* fakturagenerering
* ændring af fakturastatus

Adgangskontrollen placeres i API’et og ikke kun i frontenden. Dette er vigtigt, fordi frontendvalidering alene ikke kan betragtes som sikker adgangskontrol. En bruger kan potentielt kalde API’et direkte, og derfor skal API’et selv håndhæve autorisation.

Dette understøtter secure by design-princippet om, at sikkerhed skal håndhæves tæt på de beskyttede ressourcer.

## Password hashing

Administratorpasswords lagres ikke i plaintext. Systemet anvender ASP.NET Cores `PasswordHasher`, som bygger på PBKDF2-baseret password hashing.

Denne tilgang blev valgt frem for simpel hashing, fordi PBKDF2 understøtter:

* saltede password hashes
* work factor
* bedre modstand mod brute-force-angreb
* framework-understøttet password-verifikation

`PasswordHasher` accepterer også resultatet `SuccessRehashNeeded`, fordi det betyder, at passwordet er korrekt, selvom frameworket anbefaler rehashing med opdaterede parametre.

Dette designvalg gør passwordhåndteringen mere robust og mere i tråd med moderne sikkerhedspraksis end simpel SHA256-hashing.

## Login rate limiting

Login-endpointet er beskyttet med fixed-window rate limiting.

Formålet er at reducere risikoen for brute-force-angreb mod administratorlogin. Systemet begrænser antallet af loginforsøg pr. IP-adresse inden for et fast tidsvindue.

Hvis grænsen overskrides, returnerer API’et `429 Too Many Requests`.

Rate limiting fungerer som et ekstra sikkerhedslag og supplerer password hashing og autentificeringslogik.

## Inputvalidering

Inputvalidering anvendes som en central del af sikkerhedsdesignet.

Systemet validerer input på flere niveauer:

* DTO-validering
* model validation i API’et
* defensive checks i service-laget
* database constraints

Eksempler på validering:

* obligatoriske felter
* gyldige datoer
* positive energiværdier
* gyldige statusovergange
* gyldige relationer mellem kunde, periode, ladepost og faktura

Denne lagdelte valideringsstrategi reducerer risikoen for ugyldige data og beskytter systemet mod fejltilstande, som kan føre til inkonsistente faktura- eller importdata.

## Beskyttelse af følsomme data

Systemet håndterer data, der kan være følsomme i konteksten af et bofællesskab, eksempelvis:

* kundedata
* forbrugsdata
* fakturagrundlag
* loginoplysninger

Sikkerhedsdesignet fokuserer derfor på at begrænse eksponering af data gennem:

* rollebaseret adgang
* DTO’er
* API-validering
* begrænset frontendadgang
* sikker håndtering af tokens og cookies

DTO’er anvendes til at kontrollere, hvilke data der returneres til klienten, og til at undgå direkte eksponering af database-entiteter.

## ## Sikker publicering af repository

Da projektet publiceres på GitHub som portfolio- og eksamensprojekt, blev repository-sikkerhed også behandlet som en del af sikkerhedsarbejdet.

Før publicering blev:

* udviklingssecrets fjernet
* Git-historik ryddet for gamle secrets
* lokale konfigurationsfiler udelukket via `.gitignore`
* placeholder-værdier anvendt i public-safe konfiguration

Dette reducerer risikoen for eksponering af credentials og viser samtidig, at sikkerhed ikke kun handler om runtime-systemet, men også om udviklingsproces og source control.

## Sikkerhedsmæssige afgrænsninger

MVP-versionen indeholder ikke alle sikkerhedstiltag, som ville være relevante i et fuldt produktionsmodent system.

Følgende områder er bevidst afgrænset eller udskudt:

* refresh tokens
* session revocation
* distribueret session store
* avanceret audit logging
* kunde-login
* MitID/SSO
* detaljeret permission-model
* penetration testing
* fuld GDPR-analyse

Disse fravalg skyldes projektets scope og fokus på en realistisk MVP, hvor de vigtigste sikkerhedsmekanismer omkring administratoradgang, inputvalidering og beskyttelse af faktura- og forbrugsdata prioriteres først.

Sikkerhedsdesignet giver dermed et solidt fundament, som kan udvides ved videreudvikling af systemet.

---

# 6. Software Quality og Testbarhed

Software Quality har været et centralt designmål gennem hele udviklingsforløbet. Systemets arkitektur, lagdeling og afhængighedsstruktur er bevidst udviklet med fokus på testbarhed, løs kobling og kontrollerbar forretningslogik.

Projektet fokuserer særligt på:

* design for testability
* unit tests
* integration tests
* white-box testing
* coverage-analyse
* systematisk testdesign
* realistisk verifikation af forretningsregler

Teststrategien er udviklet parallelt med implementeringen af systemets centrale workflows og fungerer som integreret del af arkitekturen fremfor som efterfølgende verificering.

## Design for Testability

Systemets arkitektur er udviklet med design for testability som centralt princip.

Formålet med denne tilgang er at sikre, at:

* forretningslogik kan testes isoleret
* komponenter kan udskiftes under test
* kontrolflows kan verificeres systematisk
* fejlscenarier kan reproduceres kontrolleret

Dette understøttes gennem:

* lagdelt arkitektur
* dependency injection
* interfaces
* isoleret service-logik
* repository-abstraktioner

Controllerne indeholder bevidst minimal forretningslogik, mens størstedelen af systemets regler er placeret i service-laget. Denne opdeling gør det muligt at teste systemets vigtigste workflows uden direkte afhængighed til HTTP-laget eller frontendkomponenter.

## Unit Tests

Service-laget testes primært gennem unit tests med fokus på isoleret verifikation af forretningslogik.

Under unit tests erstattes afhængigheder med mocks ved hjælp af Moq. Dette gør det muligt at:

* isolere services
* kontrollere afhængigheder
* fremprovokere fejlscenarier
* verificere branches og exception paths

Eksempelvis testes:

* fakturagenerering
* statusflows
* beskyttelse mod dubletter
* importlogik
* autentificeringsflow
* password-verifikation

`InvoiceCalculationService` testes uden mocks, fordi servicen udelukkende indeholder ren beregningslogik uden eksterne afhængigheder.

Denne opdeling mellem:

* ren logik
* afhængighedstunge services

gjorde det muligt at vælge forskellige teststrategier afhængigt af komponenternes ansvar og kompleksitet.

## Integration Tests

Repository-laget testes gennem integration tests mod SQLite in-memory.

Integration testing var ikke oprindeligt et centralt fokusområde i projektets scope, men blev gradvist prioriteret højere under udviklingsforløbet, efterhånden som behovet for realistisk verifikation af relationel databaseadfærd blev tydeligere.

SQLite blev valgt frem for EF Core InMemory, fordi SQLite i højere grad efterligner reel relationel databaseadfærd.

Dette viste sig blandt andet ved:

* håndhævelse af foreign key constraints
* realistisk persistensadfærd
* relationel validering
* mere troværdige queries

Under testarbejdet afslørede SQLite fejl i testopsætningen, hvor relaterede entities ikke var oprettet korrekt før persistens af ladeposter.

Denne type fejl ville ikke nødvendigvis være blevet identificeret med EF Core InMemory, fordi InMemory-provider’en ikke fungerer som en fuld relationel database.

Valget af SQLite styrkede dermed:

* integrationstestenes troværdighed
* realistisk databaseverifikation
* kvaliteten af persistenslaget

## Mocking og Dependency Injection

Dependency injection spiller en central rolle i projektets teststrategi.

Services og repositories registreres centralt i `Program.cs` og injiceres via constructor injection. Dette gør det muligt at erstatte konkrete implementeringer med mocks under unit tests.

Moq anvendes blandt andet til:

* repositories
* sikkerhedskomponenter
* JWT-services
* hjælpekomponenter

Denne tilgang understøtter:

* isoleret testdesign
* kontrollerede fejlscenarier
* verificering af service-adfærd
* reduceret kobling mellem komponenter

Arkitekturen blev dermed ikke kun designet for runtime-funktionalitet, men også for at understøtte systematisk testarbejde.

## White-box Testing

White-box testing anvendes på centrale metoder i service- og importlogikken med fokus på:

* kontrolflow
* branches
* exception paths
* loops
* beslutningspunkter

Testene designes ud fra kendskab til metodernes interne struktur fremfor udelukkende ud fra input og output.

Der arbejdes blandt andet med:

* statement coverage
* branch coverage
* condition testing
* path testing
* exception path testing
* loop testing
* data flow testing

Særligt følgende metoder blev analyseret:

* `InvoiceService.GenerateAsync`
* `InvoiceService.UpdateStatusAsync`
* `ChargingSessionCsvImportService.ImportAsync`
* `ChargingSessionCsvImportService.ParseAsync`
* `CustomerService.CreateAsync`

Disse metoder blev valgt, fordi de indeholder centrale beslutningspunkter relateret til:

* kundetjek
* periodevalidering
* beskyttelse mod dubletter
* statusflows
* importlogik
* continue-paths

White-box testing fungerede dermed som aktivt grundlag for testdesign fremfor blot som efterfølgende verificering.

## Cyclomatic Complexity og DD-path Analyse

Der blev udført analyse af cyclomatic complexity og DD-path graphs på udvalgte metoder for at identificere:

* uafhængige kontrolforløb
* centrale branches
* nødvendige test paths

Særligt `InvoiceService.GenerateAsync` blev analyseret, fordi metoden indeholder flere kritiske beslutningspunkter relateret til:

* kundetjek
* periodevalidering
* dubletkontrol
* håndtering af ladeposter

Metoden blev analyseret ud fra McCabes formel:

V(G)=E−N+2P

Analysen blev anvendt som støtte til:

* branch coverage
* testdesign
* identifikation af nødvendige kontrolforløb

Cyclomatic complexity og DD-path analyse blev dermed anvendt som praktiske værktøjer til struktureret white-box testing fremfor blot som teoretiske kompleksitetsmålinger.

## Coverage-analyse

Der blev udført coverage-analyse ved hjælp af:

* `coverlet.collector`
* `ReportGenerator`

Testresultatet bestod af 71 beståede tests uden fejl.

Den samlede coverage viste:

* 41,2 % line coverage
* 22,1 % branch coverage

Coverage blev anvendt som støtteværktøj til at identificere uprøvede områder i systemet, men ikke som eneste kvalitetsmål.

Den samlede coverage trækkes blandt andet ned af:

* controllers
* `Program.cs`
* framework-genereret OpenAPI-kode

Disse områder var ikke hovedfokus i projektets teststrategi.

Testindsatsen blev i stedet prioriteret mod:

* service-lag
* persistenslogik
* sikkerhedskomponenter
* kritiske forretningsregler

Flere centrale komponenter opnåede væsentligt højere coverage end systemets samlede gennemsnit.

Eksempelvis opnåede:

* `AuthService` 100 % line coverage og 100 % branch coverage
* `ChargingSessionCsvImportService` 100 % line coverage og 100 % branch coverage
* `JwtService` 100 % line coverage
* `PasswordHasher` 100 % line coverage

`InvoiceService` opnåede samtidig:

* 66,4 % line coverage
* 86,1 % branch coverage

på centrale kontrolflows.

Coverage-resultaterne understøtter dermed, at teststrategien primært har været rettet mod de komponenter, hvor fejl vurderes at have størst betydning for:

* korrekt funktionalitet
* datakonsistens
* sikkerhedsrelateret adfærd

## Teststrategiske Designmål

Projektets teststrategi blev udviklet med fokus på:

* realistisk verifikation af forretningslogik
* kontrollerbar testopsætning
* reproducerbare fejlscenarier
* tydelig kobling mellem arkitektur og testbarhed

Software Quality blev dermed behandlet som integreret del af systemdesignet fremfor som separat testfase efter implementering.

Arkitekturen, dependency injection-strukturen og lagdelingen blev alle udviklet med henblik på at understøtte systematisk testarbejde og kontrolleret verifikation af systemets vigtigste workflows og forretningsregler.

---

# 7. Database- og Persistensdesign

Persistenslaget i Ladestander-systemet er udviklet med fokus på:

* relationel integritet
* datakonsistens
* vedligeholdbarhed
* kontrolleret persistens
* realistisk håndtering af historiske fakturadata

Systemet anvender Microsoft SQL Server som primær database og Entity Framework Core som ORM-lag mellem backend-systemet og databasen.

Databasedesignet understøtter både:

* forretningslogik
* teststrategi
* sikkerhedsovervejelser
* robust håndtering af lade- og fakturadata

## Microsoft SQL Server som Persistenslag

Microsoft SQL Server blev valgt som persistenslag for at understøtte:

* relationel datamodellering
* foreign key constraints
* indeksoptimering
* transaktionel konsistens
* stabil integration med .NET og Entity Framework Core

Systemet håndterer:

* kunder
* perioder
* ladeposter
* fakturaer
* administratorbrugere

som relationelle entities med tydelige relationer mellem objekterne.

Microsoft SQL Server fungerer samtidig som fundament for:

* datakonsistens
* historisk fakturering
* persistens af forretningskritiske data

## Entity Framework Core

Persistenslaget er implementeret med Entity Framework Core.

`AppDbContext` fungerer som centralt databasekontekstlag og håndterer:

* entity mapping
* relationer
* persistens
* migrations
* databaseopdateringer

Entity Framework Core blev valgt for at:

* reducere manuel SQL-kode
* understøtte repository-pattern
* forbedre udviklingshastighed
* gøre persistenslaget lettere at vedligeholde

Samtidig giver Entity Framework Core mulighed for:

* LINQ-baserede queries
* central konfiguration af relationer
* databaseuafhængige integration tests via SQLite

## Relationer og Foreign Keys

Datamodellen er designet med tydelige relationer mellem centrale entiteter.

Eksempelvis:

* tilhører en ladepost én kunde
* tilhører en ladepost én periode
* kan en ladepost være koblet til én faktura
* tilhører en faktura én kunde
* tilhører en faktura én periode

Disse relationer håndhæves gennem foreign keys og relationel integritet i databasen.

Foreign key constraints anvendes til at forhindre:

* orphaned records
* ugyldige relationer
* inkonsistente persistensscenarier

Under integration tests viste foreign key constraints sig samtidig som vigtig mekanisme til identifikation af fejl i persistensflowet.

## Constraints og Datakonsistens

Databasen anvender flere constraints for at beskytte systemets datakonsistens.

Eksempelvis anvendes:

* foreign key constraints
* unikke constraints
* nullable relationer hvor relevant

En central constraint i systemet er:

* én faktura pr. kunde pr. periode

Dette håndhæves gennem unik constraint på:

* `CustomerId`
* `BillingPeriodId`

Dermed forhindres dubletfakturering direkte på databaseniveau.

Der anvendes samtidig beskyttelse mod dubletter for ladeposter gennem unik constraint baseret på:

* kunde
* periode
* starttidspunkt
* ladenavn

Denne strategi reducerer risikoen for dobbeltimport af ladeposter fra CSV-filer.

## Nullable Relationer

`ChargingSession` indeholder nullable relation til `Invoice`.

Dette design blev valgt, fordi:

* ladeposter importeres før fakturagenerering
* import og fakturering er separate workflows
* persistenslaget skal understøtte mellemtilstande

Dermed kan ladeposter eksistere i databasen uden at være faktureret endnu.

Dette understøtter et mere fleksibelt og realistisk workflow for import og fakturering.

## Snapshot-data på Fakturaer

Fakturaer gemmer snapshot-data som:

* `TotalEnergyKWh`
* `TotalAmount`

Snapshot-strategien blev valgt for at sikre historisk stabilitet i fakturadata.

Hvis systemet i stedet beregnede fakturadata dynamisk ud fra aktuelle ladeposter og aktuelle energipriser, ville historiske fakturaer potentielt ændre sig over tid.

Ved at gemme snapshot-data sikres det, at:

* historiske fakturaer forbliver stabile
* ændringer i energipriser ikke påvirker gamle fakturaer
* senere ændringer i ladeposter ikke omskriver historiske fakturabeløb

Dette designvalg understøtter:

* revisionssikkerhed
* historisk datakonsistens
* mere robust persistens

## Indexes og Query-optimering

Databasen anvender indexes på udvalgte relationer og søgefelter.

Eksempelvis anvendes indeks på:

* kunde- og periodekombinationer
* relationer mellem ladeposter og fakturaer
* søgninger anvendt i fakturagenerering og importlogik

Formålet er at:

* reducere query-tid
* forbedre persistensperformance
* understøtte hyppige opslag i service-laget

Selvom MVP-versionen kun håndterer et relativt begrænset datamængde, blev databasen stadig designet med fokus på realistisk relationel struktur og fremtidig udvidelse.

## Persistens og Repository-design

Persistensadgang sker gennem repositories som:

* `CustomerRepository`
* `BillingPeriodRepository`
* `ChargingSessionRepository`
* `InvoiceRepository`
* `AdminUserRepository`

Repositories fungerer som abstraktionslag mellem:

* service-lag
* Entity Framework Core
* databasen

Denne struktur blev valgt for at:

* understøtte dependency injection
* forbedre testbarhed
* reducere koblingen mellem forretningslogik og persistens
* gøre persistensflow mere kontrollerbart

Repositories eksponeres gennem interfaces for at gøre det muligt at:

* mocke persistens under unit tests
* udskifte implementationer senere
* isolere databaseadgang under white-box testing

## SQLite i Integration Tests

SQLite in-memory anvendes i integration tests som realistisk relationel testdatabase.

SQLite blev valgt frem for EF Core InMemory, fordi SQLite:

* håndhæver foreign key constraints
* understøtter relationel databaseadfærd
* giver mere realistiske persistensscenarier

Denne beslutning viste sig vigtig under testarbejdet, hvor SQLite afslørede fejl i relationel persistens, som EF Core InMemory ikke nødvendigvis ville have identificeret.

Persistensdesignet blev dermed ikke kun udviklet med fokus på runtime-funktionalitet, men også med fokus på realistisk og kontrollerbar databaseverifikation under test.

---

# 8. Fejlhåndtering og Robusthed

Fejlhåndtering og robusthed har været centrale designmål gennem udviklingen af Ladestander-systemet. Systemet håndterer både:

* persistens
* importflow
* autentificering
* fakturagenerering
* statusflows
* databaseoperationer

og flere af disse områder involverer potentielt fejlbehæftede eller inkonsistente inputscenarier.

Systemets fejlstrategi fokuserer derfor på:

* kontrollerede fejlscenarier
* tydelige fejlresponses
* defensive checks
* validering på flere niveauer
* reduceret risiko for inkonsistente data
* kontrolleret håndtering af exceptions

## Validation-strategi

Systemet anvender en lagdelt valideringsstrategi bestående af:

* DTO-validering
* model validation
* service-validering
* database constraints

Denne struktur blev valgt for at reducere risikoen for:

* ugyldige requests
* inkonsistente entities
* ugyldige statusovergange
* persistensfejl
* fejl i importflow

### DTO-validering

DTO’er anvender blandt andet:

* `Required`
* `Range`
* `StringLength`
* `Pattern`

til at validere request-data tidligt i API-flowet.

Denne type validering anvendes eksempelvis til:

* obligatoriske felter
* gyldige datoformater
* numeriske grænser
* inputstruktur

Tidlig validering reducerer risikoen for, at ugyldige requests når ind i service-laget.

### Service-validering

Forretningskritiske regler håndteres samtidig i service-laget gennem defensive checks.

Eksempler:

* perioder må ikke være lukkede ved import
* fakturaer må ikke oprettes dobbelt
* Paid-fakturaer må ikke ændres
* ladeposter skal være koblet til gyldige kunder
* ugyldige statusovergange afvises

Denne type validering blev placeret i service-laget for at:

* centralisere forretningsregler
* reducere duplikeret logik
* gøre regler lettere at teste
* understøtte white-box testing

## Defensive Checks

Systemet anvender defensive checks i centrale workflows for at reducere risikoen for:

* null-scenarier
* ugyldige relationer
* persistensfejl
* fejl i importflow

Eksempelvis udføres checks for:

* manglende kunder
* manglende perioder
* tomme imports
* dubletter
* ugyldige statusværdier
* ugyldige relationer mellem entities

Defensive checks anvendes som ekstra sikkerhedslag ud over almindelig inputvalidering.

## CSV-import og Robusthed

CSV-importflowet er designet med fokus på robusthed og kontrolleret fejlhåndtering.

Importlogikken håndterer blandt andet:

* ugyldige CSV-rækker
* manglende kunder
* ugyldige perioder
* dublerede ladeposter
* parsing-fejl
* continue-paths ved enkeltfejl

Formålet er at sikre, at enkelte fejl ikke nødvendigvis stopper hele importprocessen.

Importflowet registrerer samtidig:

* importerede rækker
* oversprungne rækker
* fejlårsager

Denne strategi gør importprocessen:

* mere robust
* lettere at debugge
* lettere at analysere under test

## Exception Handling

Systemet anvender centraliseret exception handling gennem globale exception handlers.

Denne tilgang blev valgt for at:

* undgå duplikeret fejlhåndtering i controllers
* sikre konsistente API-fejl
* reducere controller-kompleksitet
* centralisere fejlresponses

Exception handlers anvendes blandt andet til:

* valideringsfejl
* persistensfejl
* gateway-fejl
* autentificeringsfejl
* databasefejl

Centraliseringen reducerer samtidig risikoen for inkonsistente eller uklare fejlresponses på tværs af API’et.

## API-fejl og Statuskoder

Systemet returnerer kontrollerede HTTP-statuskoder afhængigt af fejltype og kontekst.

Eksempler:

* `400 Bad Request`
* `401 Unauthorized`
* `403 Forbidden`
* `404 Not Found`
* `409 Conflict`
* `429 Too Many Requests`

Fejlresponses returneres som strukturerede objekter med information om:

* fejltype
* fejlmeddelelse
* valideringsdetaljer hvor relevant

Denne struktur gør API’et:

* mere forudsigeligt
* lettere at integrere mod
* lettere at debugge

## Persistensfejl og Datakonsistens

Databasen anvender:

* foreign key constraints
* unikke constraints
* relationel integritet

som ekstra sikkerhedslag mod inkonsistente persistensscenarier.

Eksempelvis forhindres:

* dubletfakturaer
* ugyldige relationer
* orphaned records

direkte på databaseniveau.

Denne strategi betyder, at datakonsistens ikke alene afhænger af frontend- eller service-validering.

## Logging-overvejelser

Projektet anvender relativt begrænset runtime-logging i MVP-versionen.

Dette valg blev truffet for at:

* reducere støj i logs
* holde fokus på centrale fejlscenarier
* undgå overdreven debug-logging

Eksempelvis blev health checks designet til kun at returnere relevante fejltilstande frem for kontinuerlig polling og støjende logoutput.

Systemet er dermed designet med fokus på kontrolleret og relevant fejlinformation frem for maksimal logmængde.

## Kontrollerede Fejlscenarier

En central designmålsætning har været, at systemet skal fejle kontrolleret frem for ukontrolleret.

Det betyder blandt andet:

* valideringsfejl håndteres som normale workflows
* ugyldige statusændringer afvises kontrolleret
* importfejl registreres uden nødvendigvis at stoppe hele importen
* persistensfejl håndteres centralt
* ugyldige requests returnerer tydelige fejlresponses

Denne tilgang reducerer risikoen for:

* uforudsigelig systemadfærd
* skjulte fejl
* datainkonsistens
* vanskeligt reproducerbare fejlscenarier

Fejlhåndtering og robusthed blev dermed behandlet som integreret del af systemdesignet frem for som separat fejlmekanisme tilføjet efter implementering.

---

# 9. Refleksion og Designmæssige Tradeoffs

Udviklingen af Ladestander-systemet involverede en række arkitektur- og designmæssige beslutninger, hvor fokus løbende blev afvejet mellem:

* kompleksitet
* sikkerhed
* testbarhed
* vedligeholdbarhed
* udviklingstid
* realistisk MVP-scope

Projektet blev udviklet som både:

* praktisk administrationsløsning
* lærings- og demonstrationsprojekt inden for Software Quality og Secure Software Development

Flere designvalg blev derfor truffet med fokus på:

* tydelig arkitektur
* forklarlighed
* testbarhed
* kontrollerbar kompleksitet

frem for maksimal feature-mængde eller produktionsmodenhed.

## Hvorfor .NET og Blazor

Projektet blev udviklet i .NET 10 med ASP.NET Core og Blazor.

Denne teknologistak blev valgt på baggrund af:

* stærk integration mellem frontend og backend
* moden dependency injection-understøttelse
* stærk støtte til testbarhed
* integreret sikkerhedsunderstøttelse
* tæt integration med Entity Framework Core
* stærk tooling i Visual Studio

Blazor blev valgt som frontendteknologi, fordi:

* frontend og backend kunne udvikles i samme økosystem
* C# kunne anvendes gennem hele løsningen
* server-side rendering reducerede frontendkompleksitet
* cookie-baseret autentificering kunne integreres relativt enkelt

Valget af Blazor Server reducerede samtidig behovet for avanceret JavaScript-baseret state management i MVP-versionen.

## Hvorfor REST-baseret API

Systemet blev designet omkring et separat REST-baseret API frem for direkte databaseadgang fra frontend.

Dette design blev valgt for at:

* centralisere forretningslogik
* centralisere sikkerhed
* understøtte testbarhed
* reducere koblingen mellem frontend og persistens
* gøre systemet lettere at videreudvikle

API-laget fungerer dermed som kontrolleret integrations- og sikkerhedslag mellem klient og database.

Denne struktur understøtter samtidig:

* white-box testing af services
* isoleret backend-udvikling
* fremtidige integrationsmuligheder
* potentiel udskiftning af frontendteknologi

## Hvorfor lagdelt arkitektur

Lagdelt arkitektur blev valgt for at understøtte:

* separation of concerns
* løs kobling
* testbarhed
* vedligeholdbarhed

Ved at opdele systemet i:

* controllers
* services
* repositories
* persistenslag

blev det muligt at isolere ansvar og reducere afhængigheder mellem systemets komponenter.

Denne opdeling gjorde det samtidig lettere at:

* mocke afhængigheder
* udføre unit tests
* analysere kontrolflows
* udføre white-box testing
* vedligeholde forretningsregler centralt

Arkitekturen blev dermed ikke kun valgt ud fra runtime-funktionalitet, men også ud fra Software Quality-overvejelser.

## Hvorfor SQLite til Integration Tests

En vigtig erfaring under udviklingsforløbet var, at EF Core InMemory ikke gav tilstrækkeligt realistisk databaseadfærd under integration tests.

SQLite in-memory blev derfor valgt som alternativ, fordi SQLite:

* håndhæver foreign key constraints
* understøtter relationel integritet
* opfører sig mere som en reel relationel database

Denne beslutning afslørede fejl i persistensflow og relationel opsætning, som ikke nødvendigvis ville være blevet identificeret med EF Core InMemory.

Valget af SQLite forbedrede dermed:

* troværdigheden af integration tests
* kvaliteten af persistensverifikation
* realismen i testmiljøet

Dette blev samtidig et vigtigt eksempel på, hvordan teststrategien udviklede sig under projektforløbet.

## Hvorfor manuel mapping

Projektet anvender manuel mapping mellem DTO’er og entities frem for automatiske mapping-frameworks.

Dette valg blev truffet for at:

* gøre dataflow mere eksplicit
* reducere skjult transformationslogik
* gøre debugging lettere
* øge kontrollen over API-kontrakter

Selvom automatiske mapping-frameworks potentielt kunne reducere mængden af kode, blev manuel mapping vurderet som mere transparent og lettere at analysere under test og debugging.

## Hvorfor snapshot-data på Fakturaer

Fakturaer gemmer snapshot-data som:

* samlet energiforbrug
* samlet fakturabeløb

frem for at beregne disse dynamisk ved hver læsning.

Denne beslutning blev truffet for at sikre:

* historisk stabilitet
* revisionssikkerhed
* konsistente fakturadata

Hvis systemet beregnede fakturaer dynamisk ud fra aktuelle ladeposter og energipriser, ville historiske fakturaer potentielt kunne ændre sig over tid.

Snapshot-strategien reducerer denne risiko og gør persistens mere stabil.

## Begrænsninger i MVP-versionen

Projektet blev udviklet som en MVP-løsning, og flere funktioner blev derfor bevidst afgrænset eller udskudt.

MVP-versionen indeholder eksempelvis ikke:

* kunde-login
* refresh tokens
* distribueret session management
* avanceret audit logging
* PDF-generering
* mailintegration
* automatisk dataimport fra ladeudbydere
* OCPP-integration
* UI/E2E-tests
* mutation testing

Disse områder blev fravalgt for at prioritere:

* kernefunktionalitet
* testbarhed
* sikkerhedsdesign
* arkitektur
* persistens
* realistisk scope

Projektet fokuserer dermed primært på robuste backend-principper og Software Quality frem for fuld produktionsmodenhed.

## Erfaringer fra Udviklingsforløbet

Udviklingsforløbet viste, at flere arkitektur- og testmæssige beslutninger havde større betydning end forventet.

Særligt:

* dependency injection
* repository-abstraktioner
* SQLite-baserede integration tests
* centralisering af forretningslogik

viste sig at have stor betydning for:

* testbarhed
* debugging
* fejlreproduktion
* vedligeholdbarhed

Projektet viste samtidig, at Software Quality og sikkerhedsdesign ikke fungerer optimalt som separate efterfølgende aktiviteter, men bør integreres direkte i arkitektur- og designarbejdet fra starten af udviklingsforløbet.

Flere designvalg blev derfor løbende justeret på baggrund af konkrete erfaringer fra:

* testarbejde
* persistensfejl
* importflows
* sikkerhedsarbejde
* white-box analyser

Dette gjorde udviklingsforløbet mere iterativt og bidrog til en mere moden og kontrolleret arkitektur.

---

# 10. Fremtidigt Arbejde

Selvom MVP-versionen implementerer systemets centrale funktionalitet, blev flere områder identificeret som naturlige udvidelser under udviklingsforløbet.

Projektets arkitektur blev bevidst designet med fokus på:

* løs kobling
* lagdeling
* dependency injection
* tydelige service-grænser

for at gøre videreudvikling lettere.

Flere af de fremtidige udvidelser vil kunne implementeres uden større ændringer i systemets grundlæggende arkitektur.

## Kunde-login og Rolleudvidelser

MVP-versionen understøtter kun administratorbrugere.

En naturlig videreudvikling vil være:

* kunde-login
* rollebaseret adgang for almindelige brugere
* adskillelse mellem administrator- og kundevisninger

Dette vil gøre det muligt for brugere selv at:

* se egne ladeposter
* se egne fakturaer
* følge betalingsstatus
* hente historiske data

Ved introduktion af flere roller vil systemets autorisationsmodel kunne udvides med:

* claims-baserede policies
* mere detaljeret permission-styring
* begrænsning af dataadgang pr. bruger

## Refresh Tokens og Session Management

Systemet anvender i MVP-versionen relativt simple JWT-baserede sessioner.

En mere produktionsmoden løsning ville typisk inkludere:

* refresh tokens
* token rotation
* session revocation
* central session management

Dette ville blandt andet forbedre:

* sessionkontrol
* logout-håndtering
* sikkerhed ved kompromitterede tokens

Ved større deployment-scenarier kunne distribueret session storage samtidig blive relevant.

## Automatisk Dataimport

Ladeposter importeres i MVP-versionen manuelt fra CSV-filer eksporteret fra ChargerSync.

En vigtig videreudvikling vil være automatisk integration mod:

* ladeudbyderes API’er
* cloud-platforme
* OCPP-baserede systemer

Dette ville reducere:

* manuel administration
* risiko for importfejl
* behovet for manuel filhåndtering

Samtidig ville automatisk import kunne understøtte:

* hyppigere synkronisering
* mere realtidsnær datahåndtering
* automatiseret fakturering

## PDF-generering og Mailfunktionalitet

Fakturaer håndteres i MVP-versionen primært som dataobjekter i systemet.

En naturlig udvidelse vil være:

* PDF-generering
* automatisk udsendelse af fakturaer
* mailnotifikationer
* betalingspåmindelser

Dette ville gøre løsningen mere anvendelig som fuld administrationsplatform.

## UI/E2E-tests og Mutation Testing

Projektets teststrategi fokuserer primært på:

* unit tests
* integration tests
* white-box testing

Frontendtests og end-to-end tests indgår ikke i MVP-versionen.

Ved videreudvikling kunne følgende områder styrkes:

* UI-tests
* browserbaserede integration tests
* automatiseret frontendverifikation
* mutation testing

Mutation testing ville være særligt interessant som udvidelse af Software Quality-arbejdet, fordi teknikken kan anvendes til at evaluere styrken af eksisterende tests frem for blot coverage.

## Audit Logging og Sporbarhed

MVP-versionen indeholder relativt begrænset logging.

Ved videreudvikling kunne systemet udvides med:

* audit logging
* historik over statusændringer
* logning af administratorhandlinger
* importhistorik
* revisionsspor

Dette ville især være relevant ved:

* større brugergrupper
* mere følsomme data
* højere krav til sporbarhed

## Skalering og Fremtidig Arkitektur

Systemet er udviklet til et relativt begrænset miljø med:

* to fælles ladestandere
* 20 husstande
* begrænset samtidighed

Hvis systemet senere skulle understøtte:

* flere boligforeninger
* større datamængder
* højere samtidighed
* flere brugerroller

kunne yderligere arkitekturmæssige ændringer blive relevante.

Eksempler:

* distribueret caching
* message queues
* event-baserede workflows
* containerisering
* cloud deployment
* horisontal skalering

Den nuværende lagdelte arkitektur giver dog et relativt solidt fundament for denne type videreudvikling.

## Videreudvikling af Software Quality-arbejdet

Projektet demonstrerer centrale principper inden for:

* design for testability
* dependency injection
* white-box testing
* integration testing
* sikkerhedsdesign

Men flere områder kunne videreudvikles yderligere.

Eksempler:

* højere branch coverage
* mere avanceret path testing
* automatiseret performance testing
* sikkerhedstest
* fuzz testing
* statisk kodeanalyse
* CI/CD-baseret testautomatisering

Disse områder blev bevidst afgrænset i MVP-versionen for at holde fokus på systemets kernearkitektur og centrale kvalitetsprincipper.

Projektets nuværende struktur gør det dog muligt at videreudbygge både teststrategi og sikkerhedsarbejde uden større omskrivning af systemets grundlæggende arkitektur.

---

# 11. Konklusion

Ladestander-projektet demonstrerer udviklingen af en moderne .NET-baseret webapplikation med fokus på:

* Software Quality
* Secure Software Development
* testbarhed
* lagdelt arkitektur
* sikker håndtering af forretningskritiske data

Systemet blev udviklet som en MVP-løsning til administration af opladningsdata for et bofællesskab med to fælles ladestandere og 20 husstande. Projektet erstatter en tidligere Access-baseret løsning og introducerer en mere vedligeholdbar, testbar og sikker arkitektur baseret på moderne webteknologier.

Arkitekturen blev designet med fokus på:

* separation of concerns
* dependency injection
* løs kobling
* isolerbar forretningslogik
* kontrolleret persistens
* centraliseret sikkerhed

Denne struktur understøttede både:

* systematisk testarbejde
* white-box testing
* realistisk integration testing
* vedligeholdbar videreudvikling

Projektet demonstrerede samtidig, at Software Quality og sikkerhedsdesign fungerer bedst, når de integreres direkte i arkitektur- og designarbejdet frem for at blive behandlet som efterfølgende aktiviteter.

Særligt følgende områder viste sig centrale under udviklingsforløbet:

* service-baseret forretningslogik
* dependency injection
* SQLite-baserede integration tests
* DTO-baseret API-design
* rollebaseret adgangskontrol
* kontrolleret importlogik
* centraliseret fejlhåndtering

White-box testing, coverage-analyse og cyclomatic complexity-analyse blev anvendt som praktiske værktøjer til systematisk testdesign og verificering af centrale kontrolflows og forretningsregler.

Samtidig viste projektet, at realistisk databaseadfærd gennem SQLite-baserede integration tests gav mere troværdig persistensverifikation end simplere teststrategier baseret på EF Core InMemory.

Sikkerhedsdesignet fokuserede primært på:

* JWT-baseret autentificering
* cookie-baseret sessionhåndtering
* rollebaseret adgangskontrol
* inputvalidering
* password hashing
* secure repository publishing

Disse mekanismer blev implementeret som integreret del af systemdesignet for at reducere risikoen for:

* uautoriseret adgang
* inkonsistente data
* eksponering af credentials
* fejl i forretningskritiske workflows

Projektet demonstrerer dermed, hvordan moderne backend-udvikling kan kombinere:

* testbar arkitektur
* sikkerhedsdesign
* relationel persistens
* systematisk teststrategi
* kontrolleret forretningslogik

i én samlet løsning.

Selvom MVP-versionen stadig har begrænsninger og flere områder til videreudvikling, danner arkitekturen et solidt fundament for fremtidig udvidelse af både:

* funktionalitet
* sikkerhed
* teststrategi
* integrationsmuligheder
* skalerbarhed.