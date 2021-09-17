Orbital Shell - [Technical Documentation](tech-doc.md)

<hr>
<br>

# Variables names syntaxes and operations

## bash standard:

| syntax | description | implemented
| -- | -- | -- |
| ``$var`` | value of var | ✔️
| ``${var}`` | value of var, separation from neigborought | ✔️
| ``${var:-text}`` | var if defined and not empty, else text |
| ``${var:=text}`` | vvar if defined and not empty, else text is the new name of the variable |
| ``${var:+text}`` | text if value is defined and not empty , else var replaced by '' |
| ``${var:?replacementValue}`` | if value is not defined or empty, block script and echo text if text is not empty, else display error |

## ksh standard:

| syntax | description | implemented
| -- | -- | -- |
| ``$var[n]`` | array : value at index n, 0 based | 
| ``${}`` | |
| ``${}`` | |
| ``${}`` | |

## orbsh extensions:

| syntax | description | implemented
| -- | -- | -- |
| ``$var.propertyName`` | - item of a variable group (var as a namespace)<br>- object field/property | ✔️
||- object method (call) |
