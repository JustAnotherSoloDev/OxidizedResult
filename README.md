# OxidizedResult

Oxidized Result is a library inspired by rust Result Enum.
Using this library you can easily chain multiple methods from different sources without having to worry about handling exception.

The library provides various methods to process result all the while you write code in a happy path.If there is an error in between the rest of the chained methods are not executed and you are able to get the exact exception that ocurred during the processing.

In the process chain it is possible to switch seamlessly between Sync and Async context without any extra headache.

## Usage

```
//DataLayer

public OResult<DataTable> GetUser(int id){
    //get user details
    return new Oresult<DataTable>(()=>{
        //get user 
        return user;
    })
}

//Service layer

public OResult<User> GetUser(int id){
    return DataLayer.GetUser(id).
    ProcessAsync(async(dt)=>{
        //transform the object
    })
}

public OResult<UserDetails> DetailsFromAnotherSource(User user){
        return getDetailsFromAnotherSource(userObj.id)
}

//aggregate Result
public OResult<UserDetails> GetUserDetails(int id){
    return ServiceLayer.GetUser(id)
    .ProcessAsync(ServiceLayer.DetailsFromAnotherSource);
}

//Entry point

ServiceLayer.GetUserDetails(id)
            .MatchResultAsync((result)=>{
                //handle success
                    return SuccessJson;
                },
                (exception)=>{
                    //handle exception
                    return ErrorJson;
                });

```

Simple results processing

````
//create an instance from a value
    var userData=await new OResult<int>(10)
                .ProcessAsync(async (value)=>{
                    var user=await GetUser(value);
                    return user.uuid;
                })
                .ProcessAsync(async (value)=>{
                    //get details from another source
                    var details=getDetailsFromAnotherSource(user);
                }).processAsync(async(value)=>{
                    //process the data;
                    return processedData;
                });
    if(userData.HasError){
        //handle exception
        userData.Exception
    }
    else{
        //use the user Data
        userData.Value
    }
````


### Handling Exception

You can also handle exceptions using the inbuilt MatchResult method and convert to a different type if required.

```
            someResult
                .ProcessAsync(async (value)=>{
                    //process some async Data
                    return asyncData;
                }).MatchResultAsync((result)=>{
                    //handle success
                    return success;
                },
                (exception)=>{
                    //handle exception
                    return error;
                });
```

### Unwrap a result
If you don't want to handle the result using the built in method it is possible to un warp the result at any pint in time by calling the unwrap method this will throw the captured error or return the value 

```
try{
    var result=someProcessResult.UnWrap();
    //process result
    ....
    return finalResult;
}catch(Exception ex){
    //handle exception
}

```

