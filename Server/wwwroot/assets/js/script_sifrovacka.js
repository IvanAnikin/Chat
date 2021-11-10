
aktualni_zadani = 0;
aktualni_napoveda = 0;
zadani = "";
zadaniMensi = "";
reseni = [];
napoveda = [];
var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);

$(document).ready(function () {
    $.ajaxSetup({ cache: false });
});


function Login() {

    name = document.getElementById("jmeno").value;
    surname = document.getElementById("prijmeni").value;

    //var regExp = /[a-zA-Z]/g;
    //if (regExp.test(name) && regExp.test(surname)){
    if (name !== null && name !== '' && surname !== null && surname != '') {

        var xhttp = new XMLHttpRequest();

        xhttp.onreadystatechange = function () {
            //alert(xhttp.response);
        };
        xhttp.open("PUT", "/api/Default/Sifrovacka_LogIn" + "?name=" + name + "&surname=" + surname);
        xhttp.send();

        var url = "/api/Default/Sifrovacka_Navigace"

        document.cookie = "name=" + name;
        document.cookie = "surname=" + surname;

        window.location.href = url;

    }
    else {
        alert("zadejte jméno a příjmení")
    }

}



function OnLoad(){

    name = getCookie("name");
    surname = getCookie("surname");

    if (name == null || name == '' || surname == null || surname == '') {

        var url = "/api/Default/Sifrovacka_registrace"

        window.location.href = url;

    }

    document.getElementById("user_name").innerText = name + " " + surname;

    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {

        position = xhttp.response;
        console.log(xhttp.response);

        document.getElementById("location_txt").innerText = position;

        document.getElementById("mapycz_a").href = "https://mapy.cz/turisticka?q=" + position;
        document.getElementById("googlemaps_a").href = "https://www.google.com/maps/search/?api=1&query=" + position;

    };
    xhttp.open("GET", "/api/Default/Sifrovacka_Navigace_GetLocation" + "?name=" + name + "&surname=" + surname);
    xhttp.send();

}


function OnLoadSifra() {

    name = getCookie("name");
    surname = getCookie("surname");

    if (name == null || name == '' || surname == null || surname == '') {

        var url = "/api/Default/Sifrovacka_registrace"

        window.location.href = url;

    }

    document.getElementById("user_name").innerText = name + " " + surname;

    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {

        //console.log(xhttp.responseText)
        var response = JSON.parse(xhttp.responseText);
        zadani = response.zadani;
        zadaniMensi = response.zadaniMensi;
        reseni[0] = response.reseni1;
        reseni[1] = response.reseni2;
        napoveda[0] = response.napoveda1;
        napoveda[1] = response.napoveda2;
        napoveda[2] = response.napoveda3;
        
        if (reseni[0] == "pět" || reseni[1] == "pět") {
            reseni[2] = "Pět";
            reseni[3] = "pet";
            reseni[4] = "Pet";
        }
        if (reseni[0] == "červená" || reseni[1] == "červená") {
            reseni[2] = "Červená";
            reseni[3] = "Červenou";
            reseni[4] = "Cervena";
            reseni[5] = "Cervenou";
            reseni[6] = "cervena";
            reseni[7] = "cervenou";
        }
        if (reseni[0] == "dešť" || reseni[1] == "dešť") {
            reseni[2] = "Dešť";
            reseni[3] = "Dešťová";
            reseni[4] = "Destova";
            reseni[5] = "Dest";
            reseni[6] = "destova";
            reseni[7] = "dest";
        }
        if (reseni[0] == "bernard" || reseni[1] == "bernard") {
            reseni[2] = "Bernard pub";
            reseni[3] = "bernard pub";
            reseni[4] = "Bernardyn";
            reseni[5] = "bernardyn";
            reseni[6] = "Bernardýn";
            reseni[7] = "bernardýn";
        }

        if (zadani == "ddm") {

            var url = "/api/Default/Sifrovacka_Misto"

            window.location.href = url;
        }

        document.getElementById("zadani_div").innerHTML = zadani;

        document.getElementById("napoveda1").innerHTML = napoveda[0];
        document.getElementById("napoveda2").innerHTML = napoveda[1];
        document.getElementById("napoveda3").innerHTML = napoveda[2];


        //alert(isMobile);
        if (isMobile) {

            document.getElementById("zadani1_img").style.width = "90%";
            document.getElementById("napoveda2_img").style.width = "90%";

        }

    };
    xhttp.open("GET", "/api/Default/Sifrovacka_Sifra_GetZadani" + "?name=" + name + "&surname=" + surname);
    xhttp.send();

    //alert(zadani);

    

}

function OnLoadMisto(){

    name = getCookie("name");
    surname = getCookie("surname");

    if (name == null || name == '' || surname == null || surname == '') {

        var url = "/api/Default/Sifrovacka_registrace"

        window.location.href = url;

    }

    document.getElementById("user_name").innerText = name + " " + surname;

    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {

        response = JSON.parse(xhttp.response);

        var nazev = response.nazev;
        var popis = response.popis

        //alert(xhttp.response);

        document.getElementById("misto_nazev").innerText = nazev;
        document.getElementById("misto_txt").innerText = popis;

        if (nazev == "DDM Praha 13") {
            document.getElementById("dal_btn").style.display = "none";
            document.getElementById("misto_h2").innerText = "Gratulujeme, dosáhl jsi cíle";
            document.getElementById("misto_txt").innerText = "K umístění DDM Stodůlky V Chlupově ulici vedla trojice stěhování. Prvním sídlem Místního domu dětí a mládeže Stodůlky byla bývalá knihovna v současném Spolkovém domě K Vidouli 727 . Poté proběhlo první stěhování do ulice Lýskova, kde probíhala činnost ve 3 klubovnách.. Druhé stěhování proběhlo v roce 1998 a vydrželi jsem zde hezkých 10 let. Nacházeli jsme se na kraji Centrálního parku v Bronzové ulici.. V této budově jsme využívali už 7 kluboven (pomalu, ale jistě se rozrůstáme). Třetí a snad poslední stěhování proběhlo v roce 2009. Tato budova patřila škole. S tímto stěhování jsme se také přejmenovali na Dům dětí a mládeže Stodůlky.";
        }

    };
    xhttp.open("GET", "/api/Default/Sifrovacka_Misto_Get" + "?name=" + name + "&surname=" + surname);
    xhttp.send();

}
function OnLoadVitezove(){

    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {

        response = JSON.parse(xhttp.response);

        console.log(response);

        var seznam_jmen = response.seznam_jmen;
        var seznam_prijmeni = response.seznam_prijmeni;

        var html = "";

        for (var i = 0; i < seznam_jmen.length; i++) {

            html += "<p>" + seznam_jmen[i] + " " + seznam_prijmeni[i] + "</p>"

        }

        document.getElementById("seznam_vitezu").innerHTML = html;

    };
    xhttp.open("GET", "/api/Default/Sifrovacka_Vitezove_Get");
    xhttp.send();

}


function Zkontrolovat() {

    odpoved = document.getElementById("odpoved").value;


    for (var i = 0; i < reseni.length; i++) {
        if (odpoved == reseni[i]) {
            var url = "/api/Default/Sifrovacka_Misto"

            window.location.href = url;
        }
    }

}

function ZadaniChange() {

    if (aktualni_zadani == 0) {
        document.getElementById("zadani_div").innerHTML = zadani;
        aktualni_zadani = 1;
        document.getElementById("zadani_btn").innerText = "ZADÁNÍ PRO MENŠÍ"
        if (isMobile) document.getElementById("zadani1_img").style.width = "90%";
        else document.getElementById("zadani1_img").style.width = "40%";
    }
    else {
        document.getElementById("zadani_div").innerHTML = zadaniMensi;
        aktualni_zadani = 0;
        document.getElementById("zadani_btn").innerText = "ZADÁNÍ PRO STARŠÍ"

        document.getElementById("napoveda1").style.display = "none";
        document.getElementById("napoveda2").style.display = "none";
        document.getElementById("napoveda3").style.display = "none";
    }

}


function NapovedaClick() {

    if (aktualni_napoveda == 0) {
        document.getElementById("napoveda1").style.display = "block";
        aktualni_napoveda = 1;
    }
    else if (aktualni_napoveda == 1) {
        document.getElementById("napoveda2").style.display = "block";
        aktualni_napoveda = 2
    }
    else if(aktualni_napoveda == 2){
        document.getElementById("napoveda3").style.display = "block";
        aktualni_napoveda = 3;
    }

}

function JitDal() {

    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {

        console.log(xhttp.response);

        var url = "/api/Default/Sifrovacka_Navigace"

        window.location.href = url;

    };
    xhttp.open("PUT", "/api/Default/Sifrovacka_Sifra_Submit" + "?name=" + name + "&surname=" + surname);
    xhttp.send();


}


function OnSpotClick() {

    var url = "/api/Default/Sifrovacka_Sifra"

    if (document.getElementById("location_txt").innerText == "50.0430947N, 14.3242531E") {
        url = "/api/Default/Sifrovacka_Misto"
    }

    window.location.href = url;

}





function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}