# Variables names syntaxes and operations

## bsh/ksh standard:

| syntax | description | implemented
| -- | -- | -- |
| ``$var`` | value of var | ✔️
| ``${var}`` | value of var, separation from neigborought | ✔️
| ``${var:-text}`` | var if defined and not empty, else text |
| ``${var:=text}`` | vvar if defined and not empty, else text is the new name of the variable |
| ``${var:+text}`` | text if value is defined and not empty , else var replaced by '' |
| ``${var:?replacementValue}`` | if value is not defined or empty, block script and echo text if text is not empty, else display error |
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |

## Orbsh extensions

| syntax | description | implemented
| -- | -- | -- |
| ``$var.propertyName`` | object field/property or item of a variable group (var as a namespace) | ✔️
