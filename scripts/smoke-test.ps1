param(
    [string]$ApiUrl = "http://localhost:8081",
    [string]$KeycloakUrl = "http://localhost:8080",
    [string]$ClientId = "inventory-api",
    [string]$ClientSecret = "inventory-api-secret"
)
$ErrorActionPreference = "Stop"
$script:failures = 0
function Assert-True([bool]$condition, [string]$message) {
    if ($condition) {
        Write-Host "PASS $message" -ForegroundColor Green
        return
    }
    Write-Host "FAIL $message" -ForegroundColor Red
    $script:failures++
}
function Wait-ForUrl([string]$url, [int]$seconds = 180) {
    $deadline = (Get-Date).AddSeconds($seconds)
    while ((Get-Date) -lt $deadline) {
        $ready = $false
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
            $ready = $response.StatusCode -eq 200
        } catch {
            $ready = $false
        }
        if ($ready) {
            return
        }
        Start-Sleep -Seconds 3
    }
    throw "Timed out waiting for $url"
}
function Get-HttpStatus([string]$method, [string]$url, [string]$token, [string]$bodyFile) {
    $arguments = @("-s", "-o", "NUL", "-w", "%{http_code}", "-X", $method, $url)
    if ($token) {
        $arguments += @("-H", "Authorization: Bearer $token")
    }
    if ($bodyFile) {
        $arguments += @("-H", "Content-Type: application/json", "--data-binary", "@$bodyFile")
    }
    $status = & curl.exe @arguments
    return $status
}
function Register-Movement([string]$type, [int]$quantity) {
    $body = @{
        productId = $script:productId
        type = $type
        quantity = $quantity
        unitCost = 10.00
        reason = "Smoke test"
        reference = "E2E-$script:suffix"
    } | ConvertTo-Json
    return Invoke-RestMethod -Method Post -Uri "$ApiUrl/api/inventory-movements" -Headers $script:headers -ContentType "application/json" -Body $body
}
Wait-ForUrl "$ApiUrl/health"
Wait-ForUrl "$KeycloakUrl/realms/inventory/.well-known/openid-configuration"
Write-Host "Stack is reachable. Requesting a client credentials token..." -ForegroundColor Cyan
$token = $null
for ($attempt = 0; $attempt -lt 30 -and -not $token; $attempt++) {
    try {
        $response = Invoke-RestMethod -Method Post -Uri "$KeycloakUrl/realms/inventory/protocol/openid-connect/token" `
            -Body @{ grant_type = "client_credentials"; client_id = $ClientId; client_secret = $ClientSecret } `
            -ContentType "application/x-www-form-urlencoded"
        $token = $response.access_token
    } catch {
        Start-Sleep -Seconds 3
    }
}
Assert-True ($null -ne $token) "client credentials token issued"
if (-not $token) {
    exit 1
}
$script:headers = @{ Authorization = "Bearer $token" }
$anonymousStatus = Get-HttpStatus "GET" "$ApiUrl/api/products" "" ""
Assert-True ($anonymousStatus -eq "401") "anonymous request is rejected with 401"
$swaggerStatus = Get-HttpStatus "GET" "$ApiUrl/swagger/index.html" "" ""
Assert-True ($swaggerStatus -eq "200") "Swagger UI is served"
$products = Invoke-RestMethod -Uri "$ApiUrl/api/products?pageSize=100" -Headers $script:headers
Assert-True ($products.totalCount -ge 3) "seed products are available"
$script:suffix = Get-Random -Minimum 100000 -Maximum 999999
$categoryBody = @{ name = "E2E Category $script:suffix"; description = "Smoke test category" } | ConvertTo-Json
$categoryId = Invoke-RestMethod -Method Post -Uri "$ApiUrl/api/categories" -Headers $script:headers -ContentType "application/json" -Body $categoryBody
Assert-True ($null -ne $categoryId) "category created"
$productBody = @{
    sku = "E2E-$script:suffix"
    name = "E2E Product $script:suffix"
    description = "Smoke test product"
    categoryId = $categoryId
    price = 15.75
    minimumStock = 2
} | ConvertTo-Json
$script:productId = Invoke-RestMethod -Method Post -Uri "$ApiUrl/api/products" -Headers $script:headers -ContentType "application/json" -Body $productBody
Assert-True ($null -ne $script:productId) "product created"
$product = Invoke-RestMethod -Uri "$ApiUrl/api/products/$script:productId" -Headers $script:headers
Assert-True ($product.stock -eq 0) "new product starts with zero stock"
$null = Register-Movement "In" 10
$product = Invoke-RestMethod -Uri "$ApiUrl/api/products/$script:productId" -Headers $script:headers
Assert-True ($product.stock -eq 10) "In movement increases stock to 10"
$null = Register-Movement "Out" 3
$product = Invoke-RestMethod -Uri "$ApiUrl/api/products/$script:productId" -Headers $script:headers
Assert-True ($product.stock -eq 7) "Out movement decreases stock to 7"
$movements = Invoke-RestMethod -Uri "$ApiUrl/api/inventory-movements?productId=$script:productId&pageSize=100" -Headers $script:headers
Assert-True ($movements.totalCount -eq 2) "movement history records both movements"
$insufficientBody = @{
    productId = $script:productId
    type = "Out"
    quantity = 100
    unitCost = 10.00
    reason = "Smoke test"
    reference = "E2E-$script:suffix"
} | ConvertTo-Json
$insufficientPath = Join-Path $env:TEMP "inventory-insufficient.json"
[System.IO.File]::WriteAllText($insufficientPath, $insufficientBody, (New-Object System.Text.UTF8Encoding($false)))
$insufficientStatus = Get-HttpStatus "POST" "$ApiUrl/api/inventory-movements" $token $insufficientPath
Remove-Item -Path $insufficientPath -Force
Assert-True ($insufficientStatus -eq "409") "insufficient stock returns 409"
$categoryDeleteStatus = Get-HttpStatus "DELETE" "$ApiUrl/api/categories/$categoryId" $token ""
Assert-True ($categoryDeleteStatus -eq "409") "category with active products cannot be deleted"
$productDeleteStatus = Get-HttpStatus "DELETE" "$ApiUrl/api/products/$script:productId" $token ""
Assert-True ($productDeleteStatus -eq "204") "product is soft deleted"
$categoryDeleteStatus = Get-HttpStatus "DELETE" "$ApiUrl/api/categories/$categoryId" $token ""
Assert-True ($categoryDeleteStatus -eq "204") "empty category is deleted"
if ($script:failures -gt 0) {
    Write-Host "$($script:failures) smoke check(s) failed." -ForegroundColor Red
    exit 1
}
Write-Host "All smoke checks passed." -ForegroundColor Green
