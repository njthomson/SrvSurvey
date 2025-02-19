# find all the .resx files
$resxFiles = ls *.resx -Recurse

$locBelowComment = "Localizable elements are below"

write-host "Found" $resxFiles.Length ".resx files"
foreach ($filepath in $resxFiles) {
    # load doc, then check if this marked as localizable?
    [xml]$xml = New-Object System.Xml.XmlDocument
    $xml.load($filepath)

    $metadata = $xml.SelectNodes("descendant::metadata")
    $localizable = $metadata.name -eq '$this.Localizable' -and $metadata.value -eq "True"
    if (!$localizable) { continue }

    Write-Host "Sorting: $filepath"

    # remove the comments we don't want
    foreach ($node in $xml.Root.ChildNodes | Where-Object { $_.Value -eq $locBelowComment}) {
        $null = $xml.root.RemoveChild($node)
    }

    # Collect free floating comments, we would like to keep these
    $comments = @{}
    $xml.Root.ChildNodes | Where-Object { 
        $_.ParentNode -eq $xml.Root -and 
        $_.NodeType -eq [System.Xml.XmlNodeType]::Comment -and 
        $_.NextSibling -ne $null -and 
        $_.Value -ne $locBelowComment
    } | ForEach-Object { 
        $comments[$_] = $_.NextSibling
    }

    # collect and sort relevant the nodes by their 'name' attribute
    $sortedNodes = $xml.root.ChildNodes | Where-Object { $_.LocalName -eq "data" } | Sort-Object { $_.name.Replace('$','').Replace('>>','') }

    # remove then re-add them in the correct order
    foreach ($node in $sortedNodes) { $null = $xml.root.RemoveChild($node) }
    foreach ($node in $sortedNodes) { $null =$xml.root.AppendChild($node) }

    # remove then re-add them, with a comment telling where localizable elements will begin
    $locNodes = $xml.root.ChildNodes | Where-Object { 
        $_.LocalName -eq "data" -and
        $_.Attributes["type"] -eq $null -and
        $_.Attributes["name"].Value -notlike ">*"
    }
    foreach ($node in $locNodes) { $null = $xml.root.RemoveChild($node) }
    $null = $xml.Root.AppendChild($xml.CreateComment($locBelowComment))
    foreach ($node in $locNodes) { $null =$xml.root.AppendChild($node) }

    # Restore original comments to be ahead of where they were before
    foreach ($comment in $comments.Keys) {
        $nextNode = $comments[$comment]
        $null= $comment.ParentNode.RemoveChild($comment)
        $null= $nextNode.ParentNode.InsertBefore($comment, $nextNode)
    }

    # Save the sorted XML to a new file
    $xml.Save($filepath)
}
