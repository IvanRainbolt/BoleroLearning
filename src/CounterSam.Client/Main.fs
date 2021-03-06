module CounterSam.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/FizzBuzz">] FizzBuzz

/// The Elmish application's model.
type Model =
    {
        page: Page
        counter: int
        error: string option
        fizzBuzzUpperLimit: int
    }


let initModel =
    {
        page = Home
        counter = 0
        error = None
        fizzBuzzUpperLimit = 0
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Increment
    | Decrement
    | SetCounter of int
    | SetFizzBuzzUpperNum of int
    | Error of exn
    | ClearError

let update message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.none
    | SetFizzBuzzUpperNum value ->
        { model with fizzBuzzUpperLimit = value }, Cmd.none

    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let counterPage model dispatch =
    Main.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
        .Elt()

let fizzBuzzPage model dispatch =
    Main.FizzBuzz()
        .fbIn(model.fizzBuzzUpperLimit, fun v -> dispatch (SetFizzBuzzUpperNum v))
        .GoFB(fun _ -> dispatch (SetFizzBuzzUpperNum model.fizzBuzzUpperLimit))
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Counter "Counter"
            menuItem model FizzBuzz "FizzBuzz"
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | FizzBuzz -> fizzBuzzPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        //let bookService = this.Remote<BookService>()
        //let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.none ) update view
        |> Program.withRouter router
        |> Program.withConsoleTrace
