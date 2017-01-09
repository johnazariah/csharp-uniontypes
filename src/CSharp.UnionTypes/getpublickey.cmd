@ECHO OFF
SET sn="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.2 Tools\x64\sn.exe"
%sn% -p Key.snk Key.PublicKey
%sn% -tp Key.PublicKey
PAUSE