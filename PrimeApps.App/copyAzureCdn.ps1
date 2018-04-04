$StorageAccountName = $args[0]
$StorageAccountKey = $args[1]
$StorageContainerName = $args[2]

# get Azure blob storage context
$context = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey

# get folder paths
$rootFolder = Split-Path -Path $PSScriptRoot -Parent

# get assembly version
$versionDynamic = $env:AssemblyInfo_AssemblyVersion
$versionStatic = $env:AssemblyInfo_AssemblyVersionStatic

# get views files
$viewsFolder = "$($rootFolder)\PrimeApps.App\web\views"
$filesViews = Get-ChildItem $viewsFolder -Recurse -File

Write-Host "Found $($filesViews.Count) view files."

foreach ($file in $filesViews)
{
    Write-Host "Processing $($file.FullName)"

    # get MIME type for current file
    $ContentType = "application/octetstream"

    switch ($file.Extension)
    {
        ".js" {
            $ContentType = "application/javascript"
        }
        ".css" {
            $ContentType = "text/css"
        }
        ".html" {
            $ContentType = "text/html"
        }
        ".json" {
            $ContentType = "application/json"
        }
    }

    # set Properties
    $Properties = @{"ContentType" = $ContentType; "CacheControl" = "public, max-age=31536000"}

	Write-Host "Uploaded to " $file.FullName.Replace("$($viewsFolder)\", $versionDynamic + "\web\views\")

    # upload blob
    Set-AzureStorageBlobContent `
        -File $file.FullName `
        -Blob $file.FullName.Replace("$($viewsFolder)\", $versionDynamic + "\web\views\") `
        -Context $context `
        -Container $StorageContainerName `
        -Properties $Properties `
        -Force
}

# get dist files
$distFolder = "$($rootFolder)\PrimeApps.App\web\dist"
$filesDist = Get-ChildItem $distFolder -Recurse -File

Write-Host "Found $($filesDist.Count) dist files."

foreach ($file in $filesDist)
{
    Write-Host "Processing $($file.FullName)"

    # get MIME type for current file
    $ContentType = "application/octetstream"

    switch ($file.Extension)
    {
        ".js" {
            $ContentType = "application/javascript"
        }
        ".css" {
            $ContentType = "text/css"
        }
        ".html" {
            $ContentType = "text/html"
        }
        ".json" {
            $ContentType = "application/json"
        }
    }

    # set Properties
    $Properties = @{"ContentType" = $ContentType; "CacheControl" = "public, max-age=31536000"}

	$blobName = $file.FullName.Replace("$($distFolder)\web", "")

	switch ($blobName)
    {
        "app.js" {
            $blobName = $versionDynamic + "\web\bundles-js\app.js"
        }
        "auth.js" {
            $blobName = $versionStatic + "\web\bundles-js\auth.js"
        }
        "vendor.js" {
            $blobName = $versionStatic + "\web\bundles-js\vendor.js"
        }
		"app.css" {
            $blobName = $versionDynamic + "\web\bundles-css\app.css"
        }
        "auth.css" {
            $blobName = $versionStatic + "\web\bundles-css\auth.css"
        }
        "vendor.css" {
            $blobName = $versionStatic + "\web\bundles-css\vendor.css"
        }
    }

	Write-Host "Uploaded to " $blobName

    # upload blob
    Set-AzureStorageBlobContent `
        -File $file.FullName `
        -Blob $blobName `
        -Context $context `
        -Container $StorageContainerName `
        -Properties $Properties `
        -Force
}

# get locale files
$localesFolder = "$($rootFolder)\PrimeApps.App\web\locales"
$filesLocales = Get-ChildItem $localesFolder -Recurse -File

Write-Host "Found $($filesLocales.Count) locale files."

foreach ($file in $filesLocales)
{
    Write-Host "Processing $($file.FullName)"

    # set Properties
    $Properties = @{"ContentType" = "application/json"; "CacheControl" = "public, max-age=31536000"}

	Write-Host "Uploaded to " $file.FullName.Replace("$($localesFolder)\", $versionDynamic + "\web\locales\")

    # upload blob
    Set-AzureStorageBlobContent `
        -File $file.FullName `
        -Blob $file.FullName.Replace("$($localesFolder)\", $versionDynamic + "\web\locales\") `
        -Context $context `
        -Container $StorageContainerName `
        -Properties $Properties `
        -Force
}


# get image files
$imagesFolder = "$($rootFolder)\PrimeApps.App\web\images"
$filesImages= Get-ChildItem $imagesFolder -Recurse -File

Write-Host "Found $($filesImages.Count) view files."

foreach ($file in $filesImages)
{
    Write-Host "Processing $($file.FullName)"

    # get MIME type for current file
    $ContentType = "image/jpg"

    switch ($file.Extension)
    {
        ".png" {
            $ContentType = "image/png"
        }
        ".gif" {
            $ContentType = "image/gif"
        }
        ".jpeg" {
            $ContentType = "image/jpeg"
        }
    }

    # set Properties
    $Properties = @{"ContentType" = $ContentType; "CacheControl" = "public, max-age=31536000"}

	Write-Host "Uploaded to " $file.FullName.Replace("$($imagesFolder)\", $versionStatic + "\web\images\")

    # upload blob
    Set-AzureStorageBlobContent `
        -File $file.FullName `
        -Blob $file.FullName.Replace("$($imagesFolder)\", $versionStatic + "\web\images\") `
        -Context $context `
        -Container $StorageContainerName `
        -Properties $Properties `
        -Force
}