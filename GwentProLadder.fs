open System
open System.Collections.Generic
open System.Text
open System.IO
open HtmlAgilityPack

type ProLadderItem = { rank: string; playerName: string; mmr: string; percent: string }

let urlFormat = @"https://masters.playgwent.com/en/rankings/pro-ladder/season-2/{0}"

let filePath = @"C:\Users\pawel.wicher\Desktop\pro_ladder.txt"

let getProLadderNodes (url: string) =
    let web = new HtmlWeb()
    let doc = web.Load(url)
    let rows =
        query {
            for node in doc.DocumentNode.SelectNodes("//div[@class='c-ranking-table__tr']") do
                select node
        } |> List.ofSeq
    rows

let getProLadderItem (node: HtmlNode) =
    let nodes = List.ofSeq node.ChildNodes
    let rank = nodes.[0].FirstChild.FirstChild.InnerText
    let playerName = nodes.[1].LastChild.FirstChild.InnerText
    let mmr = nodes.[2].FirstChild.FirstChild.InnerText
    {rank = rank; playerName = playerName; mmr = mmr; percent = ""}

let getProLadderItems (nodes: HtmlNode list) =
    List.map getProLadderItem nodes |> List.distinct

let getProLadderItemDisplayString (item: ProLadderItem) =
    String.Format("{0,-10}{1,-35}{2,10}{3, 10}", item.rank, item.playerName, item.mmr, item.percent)

let rec getAllProLadderItems(allItems: List<ProLadderItem>, name: string, i: int) =
    let nodes = getProLadderNodes(String.Format(urlFormat, i))
    let items = getProLadderItems nodes
    allItems.AddRange items
    if name <> items.Head.playerName then getAllProLadderItems(allItems, items.Head.playerName, i + 1)

let rec setProLadderItemsPercent(allItems: List<ProLadderItem>, i: int) =
    if i < allItems.Count then
        allItems.[i] <- { allItems.[i] with percent = (Decimal.Round(100M * Decimal.Parse(allItems.[i].rank) / Convert.ToDecimal(allItems.Count))).ToString() }
        setProLadderItemsPercent(allItems, i + 1)

let writeAllProLadderItems (allItems: List<ProLadderItem>) =
    let sb = new StringBuilder()    
    List.iter (fun x -> sb.AppendLine(getProLadderItemDisplayString x) |> ignore) (allItems |> List.ofSeq)
    File.WriteAllText(filePath, sb.ToString())

[<EntryPoint>]
let main argv =
    let allItems = new List<ProLadderItem>()
    getAllProLadderItems(allItems, "", 1)
    setProLadderItemsPercent(allItems, 0)
    writeAllProLadderItems allItems
    Console.WriteLine "Done."
    Console.ReadKey() |> ignore
    0