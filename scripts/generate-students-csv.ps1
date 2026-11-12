param(
    [Parameter(Mandatory = $true)][int]$Count,
    [Parameter(Mandatory = $true)][long]$FirstDni,
    [Parameter(Mandatory = $true)][long]$FirstLegajo,
    [Parameter(Mandatory = $true)][string]$OutFile
)

# Auxiliar de generate-students-csv.cmd: escribe el CSV (UTF-8 con BOM) con DNI y legajo consecutivos.
$ErrorActionPreference = 'Stop'

$firstNames = @(
    'Sofía', 'Mateo', 'Valentina', 'Benjamín', 'Camila', 'Joaquín', 'Martina', 'Santiago',
    'Lucía', 'Tomás', 'Catalina', 'Thiago', 'Emma', 'Bautista', 'Julieta', 'Felipe', 'Mía',
    'Franco', 'Renata', 'Lautaro', 'Agustina', 'Dante', 'Victoria', 'Nicolás', 'Abril',
    'Máximo', 'Florencia', 'Bruno', 'Josefina', 'Facundo', 'Pilar', 'Ramiro', 'Milagros',
    'Simón', 'Malena', 'Gael', 'Carolina', 'Lorenzo', 'Antonella', 'Iván', 'Clara',
    'Emiliano', 'Bianca', 'Leandro', 'Delfina', 'Benicio', 'Alma', 'Manuel', 'Paula',
    'Gabriel', 'Valeria', 'Pedro', 'Isabella', 'Federico', 'Mora', 'Ignacio', 'Elena',
    'Juan', 'Aitana', 'Rafael', 'Lola', 'Andrés', 'Olivia', 'Mariano', 'Candela'
)
$lastNames = @(
    'García', 'Rodríguez', 'González', 'Fernández', 'López', 'Martínez', 'Pérez', 'Gómez',
    'Díaz', 'Sánchez', 'Romero', 'Sosa', 'Torres', 'Álvarez', 'Ruiz', 'Ramírez', 'Flores',
    'Acosta', 'Benítez', 'Medina', 'Herrera', 'Suárez', 'Aguirre', 'Castro', 'Rojas',
    'Molina', 'Ortiz', 'Silva', 'Núñez', 'Luna', 'Cabrera', 'Ríos', 'Vega', 'Méndez',
    'Morales', 'Quiroga', 'Paz', 'Correa', 'Vargas', 'Ferreyra', 'Carrizo', 'Peralta',
    'Godoy', 'Navarro', 'Domínguez', 'Cáceres', 'Figueroa', 'Costa', 'Ponce', 'Ibarra',
    'Bravo', 'Campos', 'Arias', 'Mansilla', 'Vázquez', 'Ojeda', 'Escobar', 'Franco',
    'Miranda', 'Montenegro', 'Vera', 'Toledo', 'Agüero', 'Córdoba', 'Márquez', 'Roldán'
)

function ConvertTo-Slug([string]$text) {
    $decomposed = $text.ToLowerInvariant().Normalize([Text.NormalizationForm]::FormD)
    $chars = $decomposed.ToCharArray() | Where-Object {
        [Globalization.CharUnicodeInfo]::GetUnicodeCategory($_) -ne [Globalization.UnicodeCategory]::NonSpacingMark
    }
    -join $chars
}

$firstSlugs = $firstNames | ForEach-Object { ConvertTo-Slug $_ }
$lastSlugs = $lastNames | ForEach-Object { ConvertTo-Slug $_ }
$firstCount = $firstNames.Count
$lastCount = $lastNames.Count

$dir = Split-Path -Parent $OutFile
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }

$writer = New-Object IO.StreamWriter($OutFile, $false, (New-Object Text.UTF8Encoding($true)))
try {
    $writer.NewLine = "`r`n"
    $writer.WriteLine('DNI,Apellido,Nombre,Legajo,Teléfono,Correo')
    $buffer = New-Object Text.StringBuilder
    for ($i = 0; $i -lt $Count; $i++) {
        # Apellido recorre la lista; el nombre se desplaza cada vuelta, así las parejas tardan en repetirse.
        $last = $i % $lastCount
        $first = ($i + [math]::Floor($i / $lastCount)) % $firstCount
        $phone = if (($i + 1) % 19 -eq 0) { '' } else { '11{0:D8}' -f (55600000 + ($i % 40000000)) }
        $email = '{0}.{1}{2:D7}@edu.test' -f $firstSlugs[$first], $lastSlugs[$last], ($i + 1)
        [void]$buffer.Append($FirstDni + $i).Append(',').Append($lastNames[$last]).Append(',').Append($firstNames[$first]).Append(',').Append($FirstLegajo + $i).Append(',').Append($phone).Append(',').Append($email).Append("`r`n")
        if (($i + 1) % 5000 -eq 0) {
            $writer.Write($buffer.ToString())
            [void]$buffer.Clear()
        }
    }
    $writer.Write($buffer.ToString())
}
finally {
    $writer.Dispose()
}

Write-Host "Registros: $Count | DNI $FirstDni a $($FirstDni + $Count - 1)"
