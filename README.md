# Cache
Cache is a project which makes it possible to add a clientside cache to a Xamarin Android applikation.
## Installation
1. Clone the project where it is needed
2. Remove the Cache directory (only CacheLibary is needed for functionalities)
3. Replace Material directory (Options), IMaterialCache (CacheObjects) and MaterialDAO (DAOs.OptionsDAOs) with the object you want to Cache. (If you want to cache more than one type of object, add one directory, Interface and DAO foreach. 
4. Remove project references
## How to use
1. Add a project refererence to Cache in your existing Programm
2. Call CacheManager.Instance to get the CacheManager
3. Call GetCache with the interface in CacheObjects, to get the specific Cache
4. Call the function from interface

Hint: Usually you like to use the cache in your View. So call step 2 and 3 in constructor of your ViewModel and save your cache in a global field. When you need data from the cache, you only need to call the function and await the result.
