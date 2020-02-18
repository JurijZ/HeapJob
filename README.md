# Prepare MongoDB database (tested with MongodbDB version 3.6)


Backup Database:  
mongoexport --collection=cwjobs --db=heapjob --out="C:\HeapJob\MongoDB backup\cwjobs.json"  
mongoexport --collection=jobserve --db=heapjob --out="C:\HeapJob\MongoDB backup\jobserve.json"  
mongoexport --collection=userjobs --db=heapjob --out="C:\HeapJob\MongoDB backup\userjobs.json"  
  
Restore Database (first manually create a database named heapjob via UI):  
mongoimport --db=heapjob --collection=cwjobs --file="C:\HeapJob\MongoDB backup\cwjobs.json"  
mongoimport --db=heapjob --collection=jobserve --file="C:\HeapJob\MongoDB backup\jobserve.json"  
mongoimport --db=heapjob --collection=userjobs --file="C:\HeapJob\MongoDB backup\userjobs.json"  
  
  
Start mongo service with the database attached:  
C:\Program Files\MongoDB\Server\3.6\bin>mongod.exe --dbpath C:\HeapJob\MongoDB  
