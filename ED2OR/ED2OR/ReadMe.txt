=============================================================================================
Add Migration
=============================================================================================
add-migration migrationName 

=============================================================================================
Generate SQL for Migration
=============================================================================================
Update-Database -Script -SourceMigration: migration1 -TargetMigration: migration2

=============================================================================================
cURL Commands
=============================================================================================
curl http://api21b.easol.betaspaces.com/oauth/authorize  -d "Client_id=1BW3WaqcFv8f&Response_type=code"
curl http://api21b.easol.betaspaces.com/oauth/token -H "Content-Type: application/json" -d "{'Client_id':'1BW3WaqcFv8f','Client_secret':'1QZiYjEkUK8VhOxbVvwwnsAt','Code':'bd44176100ea49c4be341bfc47dfdd1f','Grant_type':'authorization_code'}"
curl http://api21b.easol.betaspaces.com/roster/localEducationAgencies/{LocalEducationAgency_Id}/schools
curl http://api21b.easol.betaspaces.com:80/api/v2.0/roster/localEducationAgencies/E7242CA4-BB65-453D-92E0-DC1887CDB1ED/schools -H "Authorization: Bearer 6f0bba1bb9c349d4845b70717f40dbba"
curl http://api21b.easol.betaspaces.com:80/api/v2.0/roster/schools/{School_Id}/students  -H "Authorization: Bearer 6f0bba1bb9c349d4845b70717f40dbba"
curl http://api21b.easol.betaspaces.com:80/api/v2.0/roster/schools/CEAA310E-2967-4CE2-9A15-8AD4F2D487A7/students  -H "Authorization: Bearer 6f0bba1bb9c349d4845b70717f40dbba"

