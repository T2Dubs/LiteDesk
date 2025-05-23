; -- LiteDeskInstaller.iss --
#define MyAppName "LiteDesk"
#define MyAppExeName "LiteDesk.exe"
#define MyAppPublisher "Tyler Wood"
#define MyAppVersion "1.0.0"
#define MyAppDirName "LiteDesk"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={pf}\{#MyAppDirName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=.\Output
OutputBaseFilename=LiteDeskInstaller
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\net8.0-windows\publish\win-x86\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\publish\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Code]
var
  DBTypePage: TInputOptionWizardPage;
  DBConnectionPage: TInputQueryWizardPage;
  SelectedDbType: string;
  SelectedConnStr: string;

procedure InitializeWizard;
begin
  DBTypePage := CreateInputOptionPage(wpSelectDir, 'Database Configuration', 'Choose your database type:',
    'Select the database you will use with LiteDesk.  If you only need a lightweight db to run this on one PC, choose SQLite.', True, False);
  DBTypePage.Add('SQLite');
  DBTypePage.Add('SQL Server');
  DBTypePage.Add('PostgreSQL');
  DBTypePage.Add('MySQL');

  DBConnectionPage := CreateInputQueryPage(DBTypePage.ID, 'Database Credentials', 'Enter connection details:',
    'Fill out the required connection string. This will be saved in the configuration.  If you chose SQLite and prefer the default config, you may leave this blank.');
  DBConnectionPage.Add('Connection String:', False);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  if CurPageID = DBConnectionPage.ID then
  begin
    case DBTypePage.SelectedValueIndex of
      0: SelectedDbType := 'Sqlite';
      1: SelectedDbType := 'SqlServer';
      2: SelectedDbType := 'Postgres';
      3: SelectedDbType := 'MySql';
    end;

    SelectedConnStr := DBConnectionPage.Values[0];
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  AppSettingsPath: string;
  JsonContent: string;
begin
  if CurStep = ssPostInstall then
  begin
    AppSettingsPath := ExpandConstant('{app}\appsettings.json');
    if (SelectedDbType <> 'Sqlite') or (Trim(SelectedConnStr) <> '') then
    begin
      if (SelectedDbType = 'Sqlite') and (Trim(SelectedConnStr) = '') then
        SelectedConnStr := 'Data Source=LiteDesk.db';

      JsonContent :=
        '{' + #13#10 +
        '  "Database": {' + #13#10 +
        '    "Type": "' + SelectedDbType + '",' + #13#10 +
        '    "ConnectionString": "' + SelectedConnStr + '"' + #13#10 +
        '  }' + #13#10 +
        '}';
      SaveStringToFile(AppSettingsPath, JsonContent, False);
    end;
  end;
end;
[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent