# Copilot Instructions — Minimig

Repository-specific guidance for working on Minimig. These instructions supplement (never replace) your global Copilot instructions and `.editorconfig`.

## Build and test

- Build: `dotnet build`
- Run tests: `dotnet test --project ./tests/MinimigTests.csproj --framework net10.0`
  - `global.json` enables `Microsoft.Testing.Platform` (MTP) mode for `dotnet test`. In MTP mode the project path **must** be passed via `--project`, not as a positional argument.
  - `MinimigTests.csproj` multi-targets `net8.0;net10.0` with `TestTfmsInParallel=false`; use `--framework` to target a single TFM when iterating.
- Coverage: `dotnet test --project ./tests/MinimigTests.csproj --framework net10.0 --coverlet --coverlet-output-format cobertura --results-directory ./TestResults` (via `coverlet.MTP`). Do not use `coverlet.collector` or `altcover` — both rely on the classic VSTest MSBuild target, which MTP mode bypasses, so they silently produce no coverage output.

## Running integration tests locally with Docker

`tests/Integration/ConnectionContextTests.cs` and `Integration/MigratorTests.cs` require live SQL Server, PostgreSQL, and MySQL connections — they are not mocked. Before considering a task involving these tests complete, start the local databases and run the full integration suite rather than relying solely on unit tests or a partial run.

1. Start the databases from the repo root:
   ```
   docker-compose -f .docker/docker-compose.yml --env-file .docker/.env up -d
   ```
   This provisions:
   - PostgreSQL 13 on `localhost:5432` (user `postgres`, password `Password12!`, db `postgres`)
   - MySQL (latest) on `localhost:3306` (empty root password, db `test`)
   - SQL Server 2019 on `localhost:1433` (user `sa`, password `P@ssword123`)
2. Set connection string environment variables to match the containers (the SQL Server default in tests uses a trusted connection, which does **not** work against the Docker container — it must be overridden). Use `tcp:127.0.0.1,1433` explicitly for SQL Server: if a native SQL Server instance is also installed locally, `localhost` resolves via the Shared Memory protocol to that instance instead of the Docker container, causing a login failure.
   ```powershell
   $env:Sql_Connection = "Server=tcp:127.0.0.1,1433;Database=master;User Id=sa;Password=P@ssword123;TrustServerCertificate=True;"
   $env:Postgres_Connection = "Server=localhost;Port=5432;Database=postgres;Username=postgres;Password=Password12!;"
   $env:MySql_Connection = "Server=127.0.0.1;Port=3306;Database=test;User Id=root;"
   ```
3. Run tests: `dotnet test --project ./tests/MinimigTests.csproj --framework net10.0`
4. Tear down when done: `docker-compose -f .docker/docker-compose.yml down`

A local test run failing only against these three integration tests due to connection errors (not against Docker) typically means the containers aren't running or the environment variables aren't set — start Docker and re-run before treating it as a regression.

## CI parity

`appveyor.yml` is the source of truth for the CI test/coverage invocation. When changing test tooling (test SDK, xunit, coverage provider), update `appveyor.yml` in the same change and verify the exact CI command locally before pushing.

### AppVeyor `- ps:` gotchas

- Inside `- ps:` (PowerShell) steps, read AppVeyor environment/secure variables as `$env:variable_name`. The `$(variable_name)` substitution syntax only works inside `- cmd:` steps and silently expands to an empty string in PowerShell steps (this previously broke the codecov upload with `Option 'f, file' has no value`).
- `coverlet.MTP` writes coverage files with a timestamp suffix (`coverage.cobertura.<timestamp>.xml`), never a fixed name. Do not hardcode the filename or rely on `codecov`'s automatic report discovery — the legacy `codecov` exe uploader (v1.13.0, installed via `choco install codecov`) does not recognize this timestamped pattern and fails with `No Report detected.`. Resolve it explicitly instead:
  ```powershell
  codecov -f (Resolve-Path ".\TestResults\coverage.cobertura.*.xml").Path -t $env:codecov_token
  ```
- `DOTNET_CLI_TELEMETRY_OPTOUT: '1'` and `DOTNET_NOLOGO: '1'` are set in `appveyor.yml`'s `environment:` block to keep CI logs free of the .NET telemetry notice and welcome banner. Keep these when editing that block.

## Keep docs and instructions in sync

Whenever a change affects build/test commands, tooling versions, CI configuration, Docker setup, or any other workflow described in this file, `README.md`, or `sampleinstallation.md`, update the affected documentation in the same change set. Do not leave these instructions describing a previous version of the workflow — verify the documented commands still work as written before considering the task complete.
