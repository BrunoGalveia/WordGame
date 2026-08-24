# Deploy para IIS

O workflow [`​.github/workflows/deploy.yml`](.github/workflows/deploy.yml) corre num
**self-hosted runner** (o teu PC), a cada push para `main`/`master`, e faz:

1. `dotnet publish` do backend
2. `npm run build` do frontend
3. Aplica migrações EF Core à BD ([`scripts/Deploy-Database.ps1`](scripts/Deploy-Database.ps1))
4. Copia o backend para o site IIS `WordGame-Api` ([`scripts/Deploy-Backend.ps1`](scripts/Deploy-Backend.ps1))
5. Copia o frontend para o site IIS `WordGame-Web` ([`scripts/Deploy-Frontend.ps1`](scripts/Deploy-Frontend.ps1))

Cada script pára o App Pool antes de copiar ficheiros (para o IIS largar os locks) e volta a
arrancá-lo depois — por isso há uns segundos de downtime a cada deploy, normal para um projeto
pessoal.

## 1. Pré-requisitos no Windows (uma vez só)

- **ASP.NET Core Hosting Bundle** instalado (para o IIS conseguir correr o backend .NET).
  Verifica em `C:\Program Files\IIS\Asp.Net Core Module V2\` — se não existir, instala-o
  a partir de https://dotnet.microsoft.com/download/dotnet/9.0 (secção "Hosting Bundle").
- **URL Rewrite Module** para IIS (necessário para o `web.config` do frontend fazer fallback
  das rotas do React Router) — https://www.iis.net/downloads/microsoft/url-rewrite
- **WebSocket Protocol** ativo (necessário para o SignalR funcionar através do IIS):
  ```powershell
  Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets
  ```
- Os dois sites (`WordGame-Api` em `C:\inetpub\wwwroot\wordgame-api`, `WordGame-Web` em
  `C:\inetpub\wwwroot\wordgame-web`) e respetivos App Pools já configurados — os nomes/caminhos
  usados nos scripts são os que passaste como "omissão"; ajusta os parâmetros nos passos do
  workflow se os teus forem diferentes.
- O App Pool do backend (`WordGame-Api`) deve estar em modo **No Managed Code** (o ASP.NET
  Core Module trata do runtime, não o CLR do IIS).

## 2. Variável de ligação à BD e CORS no App Pool (uma vez só)

O backend publicado lê a connection string e as origens de CORS de variáveis de ambiente do
próprio App Pool (assim nunca ficam escritas no repositório). Corre isto **uma vez** no PC
onde o IIS está instalado:

```powershell
Import-Module WebAdministration
$appPool = "WordGame-Api"

Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" `
    -Filter "system.applicationHost/applicationPools/add[@name='$appPool']/environmentVariables" `
    -Name "." -Value @{name="ConnectionStrings__Default"; value="Host=localhost;Port=5432;Database=wordgame;Username=postgres;Password=wordgame_dev_pw"}

Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" `
    -Filter "system.applicationHost/applicationPools/add[@name='$appPool']/environmentVariables" `
    -Name "." -Value @{name="Cors__AllowedOrigins__0"; value="http://<o-teu-dominio-ou-porta-do-frontend>"}

Restart-WebAppPool -Name $appPool
```

Troca `<o-teu-dominio-ou-porta-do-frontend>` pelo URL real onde o site `WordGame-Web` vai
estar acessível (ex.: `http://wordgame.local` ou `http://localhost:8080`, conforme o binding
que já tens configurado nesse site).

> Nota: a password `wordgame_dev_pw` é a mesma de desenvolvimento local — repensa-a se este
> deploy vier a sair da tua rede local.

## 3. Segredos e variáveis no GitHub (uma vez só)

No repositório: **Settings → Secrets and variables → Actions**

- **Secret** `PROD_DB_CONNECTION_STRING` — a connection string completa da BD de produção
  (a mesma instância Postgres local, mas nada impede de apontar a outra no futuro).
- **Variable** `PROD_API_URL` — o URL público do site `WordGame-Api` (ex.: `http://wordgame-api.local`
  ou `http://localhost:5001`, conforme o binding real). É gravado dentro do bundle JS do
  frontend no build, por isso tem de estar correto *antes* de correr o workflow.

## 4. Runner self-hosted

Confirma que o runner que estás a instalar:
- Corre no mesmo PC que tem o IIS e o Postgres (os scripts assumem isso).
- Tem permissões de administrador (para parar/arrancar App Pools via `WebAdministration`).
- Tem o SDK .NET 9 e o Node 22+ instalados (o workflow também os configura via
  `actions/setup-dotnet` / `actions/setup-node`, mas correm mais depressa se já estiverem lá).

## 5. Deploy manual (sem CI)

Os três scripts em [`scripts/`](scripts) também correm à mão, úteis para testar antes de
ligar o pipeline todo:

```powershell
dotnet publish server/WordGame.Api/WordGame.Api.csproj -c Release -o publish/api
cd client; npm run build; cd ..

./scripts/Deploy-Database.ps1 -ConnectionString "Host=localhost;Port=5432;Database=wordgame;Username=postgres;Password=wordgame_dev_pw"
./scripts/Deploy-Backend.ps1  -SourcePath "publish/api"  -AppPoolName "WordGame-Api" -PhysicalPath "C:\inetpub\wwwroot\wordgame-api"
./scripts/Deploy-Frontend.ps1 -SourcePath "client/dist"  -AppPoolName "WordGame-Web" -PhysicalPath "C:\inetpub\wwwroot\wordgame-web"
```
