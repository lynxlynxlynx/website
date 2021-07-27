module ElectricityChart

open Feliz
open Browser.Dom
open Fable.SimpleJson

open Utils

type DataPoint = {
    Year : int
    Total : float
    Other : float
}

type Metric = {
    Name : string
    Color : string
    Selector : DataPoint -> float
}

let metrics = [|
    { Name = "Other" ; Color = "" ; Selector = fun dp -> dp.Other }
|]

let electricityChart elementId chartKind height dataId =

    let data =
        getDataFromScriptElement dataId
        |> Json.parseNativeAs<(int * float * float * float * float * float * float * float * float * float * float * float * float * float * float * float) array>
        |> Array.map (fun (year, total, energetika_ljubljana, energetika_maribor, teb, test, tetol, tet, energetika_celje, enos, m_energetika, petrol_energetika, other, total_individual, residual, target_2030) ->
            { Year = year
              Total = total
              Other = other
            } )

    let series =
        metrics
        |> Array.map (fun metric ->
            pojo
                {| name = metric.Name
                   data = data |> Array.map (fun dp -> metric.Selector dp)
                |})

    let chartOptions =
        {| Highcharts.baseOptions with
            chart = pojo
                {| ``type`` = "column"
                   height = height
                |}
            title = null
            plotOptions = pojo
                {| column = pojo
                    {| stacking = "normal" |}
                   series = pojo
                    {| marker = pojo
                        {| enabled = false
                        |}
                    |}
                |}
            xAxis = pojo
                {| categories = data |> Array.map (fun dp -> dp.Year)
                   tickmarkPlacement = "on"
                |}
            yAxis = pojo
                {| title = pojo
                    {| text = "Emisije [kt CO<sub>2</sub>]" ; useHTML = true |} |}
            series = series
        |}

    let content =
        match chartKind with
        | "columns" -> Highcharts.chart chartOptions
        | _ -> failwith (sprintf "Unknown chart kind: %s" chartKind)

    ReactDOM.render(content, document.getElementById elementId)
