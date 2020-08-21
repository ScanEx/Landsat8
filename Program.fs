// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Net
open System.Net.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Npgsql
open NpgsqlTypes
open System.Reflection
open System.IO.Compression
open System.Xml
open System.Globalization
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type AppOptions() =
    member val Proxy = "" with get, set
    member val Timeout = 100 with get, set
    member val Url = "" with get, set
    member val BatchSize = 200 with get, set

let builder = ConfigurationBuilder()   
let config =
    builder        
        .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
        .AddJsonFile("appsettings.json")
        .Build() :> IConfiguration
let app = AppOptions()
config.GetSection("App").Bind(app)

let loggerFactory = LoggerFactory.Create(fun builder ->
    builder.AddConfiguration(config).AddConsole() |> ignore)
let logger = loggerFactory.CreateLogger() :> ILogger

type Landsat8 () =
    [<JsonProperty("panchromatic_lines")>] member val PanchromaticLines:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("nadir_offnadir")>] member val NadirOffNadir = "" with get, set
    [<JsonProperty("sunazimuth")>] member val SunAzimuth:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("reflective_samples")>] member val ReflectiveSamples:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("upperleftcornerlongitude")>] member val UpperLeftCornerLongitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("cloudcover")>] member val CloudCover:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("map_projection_l1")>] member val MapProjectionL1 = "" with get, set
    [<JsonProperty("carturl")>] member val CartURL = "" with get, set
    [<JsonProperty("sunelevation")>] member val SunElevation:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("path")>] member val Path:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("bpf_name_tirs")>] member val BpfNameTirs = "" with get, set
    [<JsonProperty("thermal_lines")>] member val ThermalLines:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("ground_control_points_model")>] member val GroundControlPointsModel:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("row")>] member val Row:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("imagequality1")>] member val ImageQuality1:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("reflective_lines")>] member val ReflectiveLines:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("ellipsoid")>] member val Ellipsoid = "" with get, set
    [<JsonProperty("geometric_rmse_model")>] member val GeometricRmseModel:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("browseurl")>] member val BrowseUrl = "" with get, set
    [<JsonProperty("browseavailable")>] member val BrowseAvailable = "" with get, set
    [<JsonProperty("dayornight")>] member val DayOrNight = "" with get, set
    [<JsonProperty("cpf_name")>] member val CpfName = "" with get, set
    [<JsonProperty("data_type_l1")>] member val DataTypeL1 = "" with get, set
    [<JsonProperty("thermal_samples")>] member val ThermalSamples:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("upperrightcornerlatitude")>] member val UpperRightCornerLatitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("lowerleftcornerlatitude")>] member val LowerLeftCornerLatitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("scenestarttime")>] member val SceneStartTime = "" with get, set
    [<JsonProperty("dateupdated")>] member val DateUpdated = "" with get, set
    [<JsonProperty("sensor")>] member val Sensor = "" with get, set
    [<JsonProperty("panchromatic_samples")>] member val PanchromaticSamples:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("ground_control_points_version")>] member val GroundControlPointsVersion:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("landsat_product_id")>] member val LandsatProductId = "" with get, set
    [<JsonProperty("acquisitiondate")>] member val AcquisitionDate = "" with get, set
    [<JsonProperty("upperrightcornerlongitude")>] member val UpperRightCornerLongitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("processing_software_version")>] member val ProcessingSoftwareVersion = "" with get, set
    [<JsonProperty("grid_cell_size_reflective")>] member val GridCellSizeReflective:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("lowerrightcornerlongitude")>] member val LowerRightCornerLongitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("lowerrightcornerlatitude")>] member val LowerRightCornerLatitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("scenecenterlongitude")>] member val SceneCenterLongitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("collection_category")>] member val CollectionCategory = "" with get, set
    [<JsonProperty("grid_cell_size_panchromatic")>] member val GridCellSizePanchromatic:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("bpf_name_oli")>] member val BpfNameOli = "" with get, set
    [<JsonProperty("scenecenterlatitude")>] member val SceneCenterLatitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("cloud_cover_land")>] member val CloudCoverLand:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("lowerleftcornerlongitude")>] member val LowerLeftCornerLongitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("geometric_rmse_model_x")>] member val GeometricRmseModelX:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("geometric_rmse_model_y")>] member val GeometricRmseModelY:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("scenestoptime")>] member val SceneStopTime = "" with get, set
    [<JsonProperty("upperleftcornerlatitude")>] member val UpperLeftCornerLatitude:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("utm_zone")>] member val UtmZone:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("date_l1_generated")>] member val DateL1Generated = "" with get, set
    [<JsonProperty("grid_cell_size_thermal")>] member val GridCellSizeThermal:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("datum")>] member val Datum = "" with get, set
    [<JsonProperty("collection_number")>] member val CollectionNumber:Nullable<int> = System.Nullable() with get, set
    [<JsonProperty("sceneid")>] member val SceneId = "" with get, set
    [<JsonProperty("rlut_file_name")>] member val RlutFileName = "" with get, set
    [<JsonProperty("tirs_ssm_model")>] member val TirsSsmModel = "" with get, set
    [<JsonProperty("roll_angle")>] member val RollAngle:Nullable<float> = System.Nullable() with get, set
    [<JsonProperty("receivingstation")>] member val ReceivingStation = "" with get, set

let toInt (value:string) =
    let x = ref 0
    if Int32.TryParse (value, NumberStyles.Integer, CultureInfo.InvariantCulture, x) then Nullable<int>(!x)
    else Nullable()

let toDouble (value:string) =
    let x = ref 0.0
    if Double.TryParse (value, NumberStyles.Float, CultureInfo.InvariantCulture, x) then Nullable<float>(!x)
    else Nullable()

let toDate (value:string) =    
    match value.Split ":" with
    | [| y; d; h; m; s; |] ->
        DateTime(int y, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddDays(float d - 1.0)
            .AddHours(float h)
            .AddMinutes(float m)
            .AddSeconds(Double.Parse(s, CultureInfo.InvariantCulture))
            .ToString("s")
    | _ -> sprintf "Ошибка разбора даты: %s" value |> failwith

let queryService url  =    
    seq {        
        use handler = new HttpClientHandler()
        handler.ServerCertificateCustomValidationCallback <- 
            fun message cert chain errors -> true
        if not(String.IsNullOrEmpty app.Proxy) then handler.Proxy <- WebProxy(app.Proxy)
        use client = new HttpClient(handler)
        client.Timeout <- TimeSpan(0, app.Timeout, 0)

        let uri = Uri(url)           
        use src =            
            client.GetStreamAsync uri
            |> Async.AwaitTask
            |> Async.RunSynchronously
        
        use gz = new GZipStream(src, CompressionMode.Decompress)                                               
        use xr = XmlReader.Create gz

        while xr.Read() do            
            if xr.Name = "metaData" then
                let item = Landsat8()
                while xr.Read() && xr.IsStartElement() do                    
                    match xr.Name.ToUpper() with
                    | "PANCHROMATIC_LINES" -> item.PanchromaticLines <- xr.ReadString() |> toInt
                    | "NADIR_OFFNADIR" -> item.NadirOffNadir <- xr.ReadString()
                    | "SUNAZIMUTH" -> item.SunAzimuth <- xr.ReadString() |> toDouble
                    | "REFLECTIVE_SAMPLES" -> item.ReflectiveSamples <- xr.ReadString() |> toInt
                    | "UPPERLEFTCORNERLONGITUDE" -> item.UpperLeftCornerLongitude <- xr.ReadString() |> toDouble
                    | "CLOUDCOVER" -> item.CloudCover <- xr.ReadString() |> toDouble
                    | "MAP_PROJECTION_L1" -> item.MapProjectionL1 <- xr.ReadString()
                    | "CARTURL" -> item.CartURL <- xr.ReadString()
                    | "SUNELEVATION" -> item.SunElevation <- xr.ReadString() |> toDouble
                    | "PATH" -> item.Path <- xr.ReadString() |> toInt                            
                    | "BPF_NAME_TIRS" -> item.BpfNameTirs <- xr.ReadString()
                    | "THERMAL_LINES" -> item.ThermalLines <- xr.ReadString() |> toInt
                    | "GROUND_CONTROL_POINTS_MODEL" -> item.GroundControlPointsModel <- xr.ReadString() |> toInt                            
                    | "ROW" -> item.Row <- xr.ReadString() |> toInt
                    | "IMAGEQUALITY1" -> item.ImageQuality1 <- xr.ReadString() |> toInt                            
                    | "REFLECTIVE_LINES" -> item.ReflectiveLines <- xr.ReadString() |> toInt                            
                    | "ELLIPSOID" -> item.Ellipsoid <- xr.ReadString()
                    | "GEOMETRIC_RMSE_MODEL" -> item.GeometricRmseModel <- xr.ReadString() |> toDouble                            
                    | "BROWSEURL" -> item.BrowseUrl <- xr.ReadString()
                    | "BROWSEAVAILABLE" -> item.BrowseAvailable <- xr.ReadString()
                    | "DAYORNIGHT" -> item.DayOrNight <- xr.ReadString()
                    | "CPF_NAME" -> item.CpfName <- xr.ReadString()
                    | "DATA_TYPE_L1" -> item.DataTypeL1 <- xr.ReadString()
                    | "THERMAL_SAMPLES" -> item.ThermalSamples <- xr.ReadString() |> toInt                            
                    | "UPPERRIGHTCORNERLATITUDE" -> item.UpperRightCornerLatitude <- xr.ReadString() |> toDouble                            
                    | "LOWERLEFTCORNERLATITUDE" -> item.LowerLeftCornerLatitude <- xr.ReadString() |> toDouble                            
                    | "SCENESTARTTIME" -> item.SceneStartTime <- xr.ReadString() |> toDate
                    | "DATEUPDATED" -> item.DateUpdated <- xr.ReadString()
                    | "SENSOR" -> item.Sensor <- xr.ReadString()
                    | "PANCHROMATIC_SAMPLES" -> item.PanchromaticSamples <- xr.ReadString() |> toInt                            
                    | "GROUND_CONTROL_POINTS_VERSION" -> item.GroundControlPointsVersion <- xr.ReadString() |> toInt                            
                    | "LANDSAT_PRODUCT_ID" -> item.LandsatProductId <- xr.ReadString()
                    | "ACQUISITIONDATE" -> item.AcquisitionDate <- xr.ReadString()
                    | "UPPERRIGHTCORNERLONGITUDE" -> item.UpperRightCornerLongitude <- xr.ReadString() |> toDouble                            
                    | "PROCESSING_SOFTWARE_VERSION" -> item.ProcessingSoftwareVersion <- xr.ReadString()
                    | "GRID_CELL_SIZE_REFLECTIVE" -> item.GridCellSizeReflective <- xr.ReadString() |> toDouble                            
                    | "LOWERRIGHTCORNERLONGITUDE" -> item.LowerRightCornerLongitude <- xr.ReadString() |> toDouble                            
                    | "LOWERRIGHTCORNERLATITUDE" -> item.LowerRightCornerLatitude <- xr.ReadString() |> toDouble                            
                    | "SCENECENTERLONGITUDE" -> item.SceneCenterLongitude <- xr.ReadString() |> toDouble                            
                    | "COLLECTION_CATEGORY" -> item.CollectionCategory <- xr.ReadString()
                    | "GRID_CELL_SIZE_PANCHROMATIC" -> item.GridCellSizePanchromatic <- xr.ReadString() |> toDouble                            
                    | "BPF_NAME_OLI" -> item.BpfNameOli <- xr.ReadString()
                    | "SCENECENTERLATITUDE" -> item.SceneCenterLatitude <- xr.ReadString() |> toDouble                            
                    | "CLOUD_COVER_LAND" -> item.CloudCoverLand <- xr.ReadString() |> toDouble                            
                    | "LOWERLEFTCORNERLONGITUDE" -> item.LowerLeftCornerLongitude <- xr.ReadString() |> toDouble                            
                    | "GEOMETRIC_RMSE_MODEL_X" -> item.GeometricRmseModelX <- xr.ReadString() |> toDouble               
                    | "GEOMETRIC_RMSE_MODEL_Y" -> item.GeometricRmseModelY <- xr.ReadString() |> toDouble                            
                    | "SCENESTOPTIME" -> item.SceneStopTime <- xr.ReadString() |> toDate
                    | "UPPERLEFTCORNERLATITUDE" -> item.UpperLeftCornerLatitude <- xr.ReadString() |> toDouble                            
                    | "UTM_ZONE" -> item.UtmZone <- xr.ReadString() |> toInt                            
                    | "DATE_L1_GENERATED" -> item.DateL1Generated <- xr.ReadString()
                    | "GRID_CELL_SIZE_THERMAL" -> item.GridCellSizeThermal <- xr.ReadString() |> toDouble                            
                    | "DATUM" -> item.Datum <- xr.ReadString()
                    | "COLLECTION_NUMBER" -> item.CollectionNumber <- xr.ReadString() |> toInt                            
                    | "SCENEID" -> item.SceneId <- xr.ReadString()
                    | "RLUT_FILE_NAME" -> item.RlutFileName <- xr.ReadString()
                    | "TIRS_SSM_MODEL" -> item.TirsSsmModel <- xr.ReadString()
                    | "ROLL_ANGLE" -> item.RollAngle <- xr.ReadString() |> toDouble 
                    | "RECEIVINGSTATION" -> item.ReceivingStation <- xr.ReadString()
                    | _ -> xr.ReadString() |> ignore
                    xr.ReadEndElement()
                yield item        
    }

let toJson (chunk:Landsat8[]) =
    let ja = JArray()
    for ls in chunk do JObject.FromObject(ls) |> ja.Add
    ja.ToString()

let save json =
    try        
        use con = new NpgsqlConnection()
        con.ConnectionString <- config.GetConnectionString "Default"
        use cmd = new NpgsqlCommand("dbo.ls_bulk")
        cmd.CommandType <- System.Data.CommandType.StoredProcedure
        cmd.Connection <- con              
        cmd.Parameters.AddWithValue(NpgsqlDbType.Json, json) |> ignore
        con.Open()
        cmd.ExecuteNonQuery () |> ignore 
    with e ->
        e.ToString() |> sprintf "json:%s,\r\n%s" json |> logger.LogError

[<EntryPoint>]
let main argv =    
    try
        queryService app.Url        
        |> Seq.chunkBySize app.BatchSize
        |> Seq.map toJson
        |> Seq.iter save
        logger.LogInformation "Сеанс завершен"
    with e ->            
        e.ToString() |> logger.LogError
    0 // return an integer exit code
