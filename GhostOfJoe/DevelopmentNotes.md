### Database
Looking to edit the underlying database? 
You will need to re-scaffold the database using the below command:

`dotnet ef dbcontext scaffold "Data Source=C:\Users\evanriker\Desktop\GhostOfJoe\hostOfJoe\GhostOfJoe\bin\ServerData.db;"  Microsoft.EntityFrameworkCore.Sqlite  --context-dir Data --output-dir Models`

This will rebuild the classes in the data and models folder. Make sure there isn't anything important in there. 
Interactions with the database should shill be in [DataHandler](DataHandler.cs)