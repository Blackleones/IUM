var quadranti = new Array(9)
var grid = new Array(9)

function createElem(name, classAttr, idAttr) {
            var elem = document.createElement(name)
            elem.setAttribute("class", classAttr)
            elem.setAttribute("id", idAttr)

            return elem
}

function init(){    
    function initGrid(){
        grid[0] = "-,-,-,-,2,-,3,5,-".split(",")
        grid[1] = "-,-,3,1,-,-,-,9,-".split(",")
        grid[2] = "6,2,-,-,8,-,-,-,-".split(",")
        grid[3] = "-,-,5,6,-,-,4,-,-".split(",")
        grid[4] = "-,3,-,8,-,5,9,2,-".split(",")
        grid[5] = "-,-,1,-,7,-,5,-,-".split(",")
        grid[6] = "-,-,2,-,-,-,1,-,7".split(",")
        grid[7] = "-,1,6,4,9,-,-,-,5".split(",")
        grid[8] = "3,-,-,7,5,-,2,-,-".split(",")        
    }
    
    function initHTML(){ 
        for(var i = 0; i < 9; i++){
            quadranti[i] = document.getElementById("q"+i)
        }
        
        var offset = 0
        for(var i = 0; i < 9; i++){
            for(var j = 0; j < 9; j++){
                var lock = ""
                if(!isNaN(grid[i][j]))
                    lock = " lock"
                    
                var box = createElem("div", "box" + lock, i + "_" + j)
                if(lock == "")
                box.addEventListener("click", game)
                var span = createElem("span", "number", i + "." + j)
                span.textContent = grid[i][j]
                box.appendChild(span)
                quadranti[parseInt(j/3)+3*parseInt(i/3)].appendChild(box)
            }
        }
    }
    initGrid()
    initHTML()
}

var game = function(e){    
    var span = e.toElement
    var str = e.toElement.toString()
    var index = []
    
    if(str.indexOf("Div") != -1){
        index = e.toElement.id.split("_")
        span = document.getElementById(index[0]+"."+index[1])
    }
    else
        index = e.toElement.id.split(".")
        
    function vRiga(n, r, c){
         for(var j = 0; j < 9; j++)
            if(c != j && grid[r][j] == n)
                return false
        return true       
    }
    
    function vCol(n, r, c){
        for(var i = 0; i < 9; i++)
            if(r != i && grid[i][c] == n)
                return false
        return true        
    }
    
    function vQuad(n, r, c){
        var startR = parseInt(r / 3) * 3 
        var startC = parseInt(c / 3) * 3
        for(var i = startR; i < startR+3; i++)
            for(var j = startC; j < startC+3; j++)
                if(grid[i][j] == n)
                    return false   
        return true        
    }
    
    function fine(){
      for(var i = 0; i < 9; i++)
            for(var j = 0; j < 9; j++)
                if(grid[i][j] == "-")
                    return false            
        return true        
    }
    
    var numero = span.innerText 
    
    if(numero == "-"){
        numero = 1
    }
    
    while(!(vRiga(numero, index[0], index[1]) &&
           vCol(numero, index[0], index[1]) &&
           vQuad(numero, index[0], index[1])) &&
          numero < 10){
        numero++
        
    } 
    
    if(numero < 10){
        grid[index[0]][index[1]] = numero
        span.innerHTML = numero
    }
    else if(numero == 10){
        span.innerHTML = "-"
        grid[index[0]][index[1]] = "-"
    }
    
    if(fine())
        alert("hai vinto!")
}

window.onload = init