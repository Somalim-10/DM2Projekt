# Lokale Booking System

booking app lavet i asp.net core med login, database og mails  
📬📅 kører live her: http://dm2projekt.anw001.dk

---

## kør det lokalt

### du skal bruge:
- visual studio 2022+
- .NET 9 sdk
- mssql localdb (typisk `(localdb)\MSSQLLocalDB`)

---

### sådan gør du:

1. clone repo  
   ```
   git clone https://github.com/KodeKammerater/DM2Projekt.git
   ```

2. åbn i visual studio

3. få `appsettings.Local.json` fra en af os  
   (den har db og mail info)

4. tilføj den til projektet – samme mappe som `appsettings.json`

5. i terminalen, skriv:  
   ```
   Update-Database
   ```

6. tryk F5 ✅

---

⚠️ dette er et public repo  
så `Local.json` og `Production.json` skal **ikke** pushes  
de ligger i `.gitignore` allerede
