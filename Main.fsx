#time "on"
#r "nuget: Akka.FSharp" 
open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp 

let N = fsi.CommandLineArgs.[1] |> int
let mutable count=0 
let ans=String.replicate N "0"
let mutable coins=0

let workers = 4
let ufid="akshithsagar"
let chars = "0123456789QRSTUVWUXYZABCDEFGHIJKLMNOPQRSTUVWUXYZabcdefghijklmnopqrstuvxyz"
let system = ActorSystem.Create("Master")
open System.Security.Cryptography
open System.Text
let hash1 (a:string)=
 a
 |> Encoding.ASCII.GetBytes
 |> (new SHA256Managed()).ComputeHash


type ActorMsg =
    | WorkerMsg of int*int*int*int
    | DispatcherMsg of int
    | EndMsg of int*int

let checkCoins j start endi id=
    let mutable ufid1=j           
    let mutable check=true
    let mutable i=0
    let mutable j=0
    
    while check do
       if coins>15 then 
            check<-false
       else 
        if j=2*chars.Length-1 then 
            check<-false
        for i in start..endi do
            let r=System.Random()

            ufid1<-ufid1+""+ string chars.[r.Next(start,chars.Length-1)]
            let x1=hash1 ufid1
            let x=System.BitConverter.ToString(x1).Replace("-","")
            if x.[0 .. N-1]=ans then
              printfn "%A %s by job %d" x ufid1 id
              coins<-coins+1 
             

            //printfn"%s for %d job"ufid1 id  

            ufid1<-ufid1.[0..ufid1.Length-2]
            ufid1<-ufid1+""+string chars.[r.Next(0,endi)]
            j<-j+1
            if ufid1.Length>15 then
                ufid1<-ufid





//worker actor
let FindBit (mailbox:Actor<_>)=
    let rec loop()=actor{
        let mutable ufid1=ufid

        let! msg = mailbox.Receive()
        match msg with 
        | WorkerMsg(start,endI,N,id) -> for j in start .. endI do
                                                        

                                                        ufid1<-ufid1+""+ string chars.[j]
                                                        checkCoins ufid1 start endI id
                                              
                                                        ufid1<-ufid1.[0..ufid1.Length-2] 
                                                        
                                                         
                                        mailbox.Sender()<! EndMsg(start,id) //send back the finish message to boss
        | _ -> printfn "Worker Received Wrong message"
    }
    loop()

// distrbutes the tasks to workers
let Dispatcher (mailbox:Actor<_>) =
    let rec loop()=actor{
        let! msg = mailbox.Receive()
        match msg with 
        | DispatcherMsg(N) ->
                                let workersList=[for a in 1 .. workers do yield(spawn system ("Job" + (string a)) FindBit)] //creating workers
                                let efforts=chars.Length/workers
                                for i in 0 .. (workers-1) do 
                                    let start=efforts*i
                                    let endI=start+efforts-1
                                    workersList.Item(i|>int) <! WorkerMsg(start,endI,N,i)
                                    //printfn" worker %d has started "i//sending message to worker
                                            
        | EndMsg(index,workerid) -> count <- count+1
                                    //printfn" worker %d has ended "workerid
                                    if count = workers then //checking if all workers have already sent the end message
                                        mailbox.Context.System.Terminate() |> ignore 
        | _ -> printfn "Dispatcher Received Wrong message"
        return! loop()
    }
    loop()
//creating boss actor
let DispatcherRef = spawn system "Dispatcher" Dispatcher

//sending message to boss actor
DispatcherRef <! DispatcherMsg(N)
//waiting for boss actor to terminate the actor system
system.WhenTerminated.Wait()






