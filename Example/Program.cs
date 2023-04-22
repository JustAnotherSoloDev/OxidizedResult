using Example;
using Oxidized.Result;


Console.WriteLine("Start");




await new OResult<int>(1)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    //.Process(TestClass.ThrowArgumentException)
    //.ProcessOrElse(TestClass.AddOne, (exception) =>
    //{
    //    Console.WriteLine("There was an error so using default value"+exception.StackTrace);
    //    return 5;
    //})
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .Process(TestClass.AddOne)
    .ProcessAsync(async (value)=>value+1)
   // .ProcessAsync(TestClass.ThrowNotFinitneError)
    .MatchResultAsync((value) =>
    {
        Console.WriteLine("The result is " + value);
    }, (ex) =>
    {
        if (ex is NotFiniteNumberException)
        {
            Console.WriteLine("Not a finite number");
        }
        else
        {
            Console.WriteLine("There was an error while executing the pipeline " + ex.StackTrace);
        }
    });








//new OResult<int>(10).Process((val) =>
//{
//    throw new NotFiniteNumberException();
//    return 20;
//}).ProcessOrElse((val) => val, (ex) =>
//{
//    Console.WriteLine("The error is " + ex.StackTrace);
//    Console.WriteLine("---------------------------------------------------");
//    return 30;
//}).MatchResult((value) => Console.WriteLine("return value is" + value), (ex) =>  Console.WriteLine(ex.ToString())) ;
//var result = new OResult<TestClass>(async () =>
//{
//    await Task.CompletedTask;
//    var value = new TestClass();
//    value.value = 10;
//    return value;
//}).ProcessAsync(async (data) =>
//{
//    await Task.CompletedTask;
//    return data;
//}).ProcessAsync(async (value) =>
//{
//    throw new NotImplementedException();
//    return value.ToString();
//}).ProcessAsync((value) => value + "").ProcessAsync((value) => 20).ProcessAsync((value) => 11.2).ProcessAsync((value) =>
//{
//    //throw new Exception("HEy there is some error");
//    return 11;
//}).ProcessAsyncOrElse(async (val) => val.ToString(), (ex) =>
//{
//    Console.WriteLine("There was an error During pipeline run" + ex.ToString());
//    return "20";
//});



//await result.MatchResultAsync((value) => Console.WriteLine("final result is " + value), (ex) =>
//{
//    if (ex is NotImplementedException)
//    {
//        Console.WriteLine("A method is not implemented in the call chain please implement the Method");
//    }
//    else
//    {
//        Console.WriteLine("Error while processing the call chain " + ex.ToString());
//    }
//});

