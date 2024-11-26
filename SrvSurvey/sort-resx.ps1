# find all the .resx files
$resxFiles = ls *.resx -Recurse

write-host "Found" $resxFiles.Length ".resx files"
foreach ($filepath in $resxFiles) {
    # load doc, then check if this marked as localizable?
    [xml]$xml = New-Object System.Xml.XmlDocument
    $xml.load($filepath)

    $metadata = $xml.SelectSingleNode("descendant::metadata")
    $localizable = $metadata.name -eq '$this.Localizable' -and $metadata.value -eq "True"
    if (!$localizable) { continue }

    Write-Host "Sorting: $filepath"
    # collect and sort relevant the nodes by their 'name' attribute
    $sortedNodes = $xml.root.ChildNodes | Where-Object { $_.LocalName -eq "data" }  | Sort-Object { $_.name.Replace('$','').Replace('&gt;&gt;','') }

    # remove and re-add everything sorted
    foreach ($node in $sortedNodes) {
        $null = $xml.root.RemoveChild($node)
    }

    foreach ($node in $sortedNodes) {
        $null =$xml.root.AppendChild($node)
    }

    # Save the sorted XML to a new file
    $xml.Save($filepath)
}
