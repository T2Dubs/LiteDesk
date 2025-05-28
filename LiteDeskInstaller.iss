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
Source: "bin\Release\net8.0-windows\publish\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Code]
var
  DBTypePage: TInputOptionWizardPage;
  SQLiteOptionPage: TInputOptionWizardPage;
  SQLiteFilenamePage: TInputQueryWizardPage;
  DBConnectionPage: TInputQueryWizardPage;
  DBCredentialPage: TInputQueryWizardPage;
  
  SelectedDbType: string;
  SelectedConnStr: string;

function SanitizeInput(const S: string): string;
begin
  Result := S;
  StringChangeEx(Result, ';', '', True);
  StringChangeEx(Result, '"', '', True);
end;

procedure InitializeWizard;
begin
  DBTypePage := CreateInputOptionPage(wpSelectDir, 'Database Configuration', 'Choose your database type:',
    'Select the database you will use with LiteDesk.  If you do not have a database, choose SQLite for local storage.', True, False);
  DBTypePage.Add('SQLite');
  DBTypePage.Add('SQL Server');
  DBTypePage.Add('PostgreSQL');
  DBTypePage.Add('MySQL');

  SQLiteOptionPage := CreateInputOptionPage(DBTypePage.ID, 'SQLite Options', 'Use default SQLite file or specify a filename?',
    'If unsure, choose the default option.', True, False);
  SQLiteOptionPage.Add('Use default (LiteDesk.db)');
  SQLiteOptionPage.Add('Use custom filename');
  
  SQLiteFilenamePage := CreateInputQueryPage(SQLiteOptionPage.ID, 'SQLite Filename', 'Custom SQLite DB file:',
    'Specify the SQLite database filename (e.g., MyLiteDesk.db).');
  SQLiteFilenamePage.Add('SQLite DB File:', False);
  
  DBConnectionPage := CreateInputQueryPage(DBTypePage.ID, 'Database Connection', 'Enter database connection details:',
    'Enter the necessary details to build your connection string.');
  DBConnectionPage.Add('Server Name:', False);
  DBConnectionPage.Add('Port:', False);
  DBConnectionPage.Add('Database Name:', False);
  
  DBCredentialPage := CreateInputQueryPage(DBConnectionPage.ID, 'Database Credentials', 'Enter your database credentials to be used by LiteDesk:',
    'Enter the configured user id and password for LiteDesk.');
  DBCredentialPage.Add('User ID:', False);
  DBCredentialPage.Add('Password:', False);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  server, port, dbname, userid, password: string;
begin
  Result := True;
  
  if CurPageID = DBTypePage.ID then
  begin
    if DBTypePage.SelectedValueIndex = -1 then
    begin
      MsgBox('Please select a database type before continuing.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    case DBTypePage.SelectedValueIndex of
      0: SelectedDbType := 'Sqlite';
      1: SelectedDbType := 'SqlServer';
      2: SelectedDbType := 'Postgres';
      3: SelectedDbType := 'MySql';
    end;
  end;
  
  if CurPageID = SQLiteOptionPage.ID then
  begin
    if SQLiteOptionPage.SelectedValueIndex = -1 then
    begin
      MsgBox('Please select an SQLite database option before continuing.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    if SQLiteOptionPage.SelectedValueIndex = 0 then
      SelectedConnStr := 'Data Source=LiteDesk.db';
  end;
  
  if CurPageID = SQLiteFilenamePage.ID then
  begin
    if SQLiteFilenamePage.Values[0] = '' then
    begin
      MsgBox('Please enter a valid SQLite database name.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    SelectedConnStr := 'Data Source=' + SanitizeInput(SQLiteFilenamePage.Values[0]);
  end;

  if CurPageID = DBConnectionPage.ID then
  begin
    server := SanitizeInput(DBConnectionPage.Values[0]);
    port := SanitizeInput(DBConnectionPage.Values[1]);
    dbname := SanitizeInput(DBConnectionPage.Values[2]);
    
    if (server = '') or (port = '') or (dbname = '') then
    begin
      MsgBox('Please fill out all required fields (Server, Port, and Database Name).', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    
    case SelectedDbType of
      'SqlServer':
        SelectedConnStr := 'Server=' + server + ',' + port + ';Database=' + dbname + ';';
      'Postgres':
        SelectedConnStr := 'Host=' + server + ';Port=' + port + ';Database=' + dbname + ';';
      'MySql':
        SelectedConnStr := 'Server=' + server + ';Port=' + port + ';Database=' + dbname + ';';
    end;
  end;
  
  if CurPageId = DBCredentialPage.ID then
  begin
    userid := SanitizeInput(DBCredentialPage.Values[0]);
    password := SanitizeInput(DBCredentialPage.Values[1]);
    
    if (userid = '') or (password = '') then
    begin
      MsgBox('Please fill out all required fields (User ID and Password).', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    
    case SelectedDbType of
      'SqlServer':
        SelectedConnStr := SelectedConnStr + 'User Id=' + userid + ';Password=' + password + ';TrustServerCertificate=True;';
      'Postgres':
        SelectedConnStr := SelectedConnStr + 'Username=' + userid + ';Password=' + password + ';';
      'MySql':
        SelectedConnStr := SelectedConnStr + 'Uid=' + userid + ';Pwd=' + password + ';';
      end;
    end;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  if (PageID = SQLiteOptionPage.ID) then
    Result := DBTypePage.SelectedValueIndex <> 0
  else if PageId = SQLiteFilenamePage.ID then
    Result := (DBTypePage.SelectedValueIndex <> 0) or
      (SQLiteOptionPage.SelectedValueIndex = 0)
  else if PageID = DBConnectionPage.ID then
    Result := DBTypePage.SelectedValueIndex = 0
  else if PageID = DBCredentialPage.ID then
    Result := DBTypePage.SelectedValueIndex = 0
  else
    Result := False;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  AppSettingsPath: string;
  JsonContent: string;
begin
  if CurStep = ssPostInstall then
  begin
    AppSettingsPath := ExpandConstant('{app}\appsettings.json');
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

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent