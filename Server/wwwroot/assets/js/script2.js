document.addEventListener('DOMContentLoaded', function () {

    // References to all the element we will need.
    var video = document.querySelector('#camera-stream'),
        image = document.querySelector('#snap'),
        start_camera = document.querySelector('#start-camera'),
        controls = document.querySelector('.controls'),
        take_photo_btn = document.querySelector('#take-photo'),
        delete_photo_btn = document.querySelector('#delete-photo'),
        download_photo_btn = document.querySelector('#download-photo'),
        error_message = document.querySelector('#error-message'),
        send_btn = document.getElementById("sendPhotoBtn"),
        upload_btn = document.getElementById("uploadWithApiBtn");


    var sessionId = document.getElementById("sesionIdTextArea").innerHTML.toString();
    var nickName = document.getElementById("nickname").innerHTML.toString();
    var chatName = document.getElementById("header").innerHTML.toString();
    

    // The getUserMedia interface is used for handling camera input.
    // Some browsers need a prefix so here we're covering all the options
    navigator.getMedia = ( navigator.getUserMedia ||
    navigator.webkitGetUserMedia ||
    navigator.mozGetUserMedia ||
        navigator.msGetUserMedia);

    var snapp;


    if(!navigator.getMedia){
        displayErrorMessage("Your browser doesn't have support for the navigator.getUserMedia interface.");
    }
    else{

        // Request the camera.
        navigator.getMedia(
            {
                video: true
            },
            // Success Callback
            function(stream){

                // Create an object URL for the video stream and
                // set it as src of our HTLM video element.
                //video.src = window.URL.createObjectURL(stream);
				//this.video.srcObject = stream;
				//this.videoTag.current.srcObject = stream;
				
				if ('srcObject' in video) {
				  video.srcObject = stream;
				} else {
				  // Avoid using this in new browsers, as it is going away.
				  video.src = URL.createObjectURL(stream);
				}
	
                // Play the video element to start the stream.
                video.play();
                video.onplay = function() {
                    showVideo();
                };
         
            },
            // Error Callback
            function(err){
                displayErrorMessage("There was an error with accessing the camera stream: " + err.name, err);
            }
        );

    }

    function dataURLtoBlob(dataurl) {
        var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new Blob([u8arr], { type: mime });
    }


    //TESTING - not working - todo: controller to get photo 
    upload_btn.addEventListener("click", function (e) {

        const account = {
            name: "notesaccount"
        };
        var sas = document.getElementById('sasInput').value;

        const blobUri = 'https://' + account.name + '.blob.core.windows.net';

        var blobService = AzureStorage.Blob.createBlobServiceWithSas(blobUri, sas);

        //var containerName = 'test-take-send';
        var containerName = 'js-photo-upload-test';



        var blob = dataURLtoBlob(snapp);

        let file;
        var blobName = "selfie.png"; //GUID !!!

        if (!navigator.msSaveBlob) { // detect if not Edge
            file = new File([blob], blobName, { type: document.image });
        } else {
            file = new Blob([blob], { type: 'image/png' });
        }


        data.append('file', file);
        $.ajax({
            url: 'api/Video/UploadPhoto',
            processData: false,
            contentType: false,
            data: data,
            type: 'POST'
        }).done(function (result) {
            alert(result);
        }).fail(function (a, b, c) {
            console.log(a, b, c);
        });



    });

    send_btn.addEventListener("click", function (e) {

        const account = {
            name: "notesaccount"
        };
        var sas = document.getElementById('sasInput').value;

        const blobUri = 'https://' + account.name + '.blob.core.windows.net';

        var blobService = AzureStorage.Blob.createBlobServiceWithSas(blobUri, sas);

        chatName = document.getElementById("header").innerHTML.toString();
        sessionId = document.getElementById("sesionIdTextArea").innerHTML.toString();
        nickName = document.getElementById("nickname").innerHTML.toString();
        var containerName = chatName.toString();
        

        /*blobService.createContainerIfNotExists(containerName, (error, container) => {
            if (error) {
                // Handle create container error
                Console.log("Error creating container: " + error);
            } else {
                console.log(container.name);
            }
        });*/



        var blob = dataURLtoBlob(snapp);
        

        let file;
        var blobName = uuidstring() + ".png";
        

        if (!navigator.msSaveBlob) { // detect if not Edge
            file = new File([blob], blobName, { type: document.image });
        } else {
            //file = new Blob([blob], { type: document.image });
            //file = blobToFile(file, blobName)
            //file = blob;
            //file.type = 'image/png';

            file = new Blob([blob], { type: 'image/png' });
            file = blobToFile(blob, blobName);
            //file = this.blobToFile(file, blobName);
        }


        blobService.createBlockBlobFromBrowserFile(containerName,
            blobName,
            file,
            (error, result) => {
                if (error) {
                    // Handle blob error
                } else {
                    console.log('Upload is successful');
                }
            });

        var http = new XMLHttpRequest();
        var url = '/api/Chat/SendMessage';

        var params = "?text=" + blobName + "&nickname=" + nickName + "&chatName=" + containerName + "&guid=" + sessionId + "isPicture=true";
        http.open('POST', url + params, true);

        http.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');

        document.getElementById("messageInput").value = '';

        http.send(params);
        

    });
    function blobToFile(blob, name) {
        const formData = new FormData();
        formData.set('file', blob, name);
        return formData.get('file');
    }


    function uuidstring() {
        return 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    
    


    // Mobile browsers cannot play video without user input,
    // so here we're using a button to start it manually.
    start_camera.addEventListener("click", function(e){

        e.preventDefault();

        // Start video playback manually.
        video.play();
        showVideo();

    });
    
    take_photo_btn.addEventListener("click", function(e){

        e.preventDefault();

        var snap = takeSnapshot();

        snapp = snap;

        // Show image. 
        image.setAttribute('src', snap);
        image.classList.add("visible");

        // Enable delete and save buttons
        delete_photo_btn.classList.remove("disabled");
        download_photo_btn.classList.remove("disabled");

        // Set the href attribute of the download button to the snap url.
        download_photo_btn.href = snap;

        /*
        document.getElementById('results').innerHTML =
            '<h2>Here is your image:</h2>' +
            '<img id="imageIMG" src="' + snap + '"/>';
        */
        // Pause video playback of stream.
        video.pause();

    });


    delete_photo_btn.addEventListener("click", function(e){

        e.preventDefault();

        // Hide image.
        image.setAttribute('src', "");
        image.classList.remove("visible");

        // Disable delete and save buttons
        delete_photo_btn.classList.add("disabled");
        download_photo_btn.classList.add("disabled");

        // Resume playback of stream.
        video.play();

    });


  
    function showVideo(){
        // Display the video stream and the controls.

        hideUI();
        video.classList.add("visible");
        controls.classList.add("visible");
    }


    function takeSnapshot(){
        // Here we're using a trick that involves a hidden canvas element.  

        var hidden_canvas = document.querySelector('canvas'),
            context = hidden_canvas.getContext('2d');

        var width = video.videoWidth,
            height = video.videoHeight;

        if (width && height) {

            // Setup a canvas with the same dimensions as the video.
            hidden_canvas.width = width;
            hidden_canvas.height = height;

            // Make a copy of the current frame in the video on the canvas.
            context.drawImage(video, 0, 0, width, height);

            // Turn the canvas image into a dataURL that can be used as a src for our photo.
            return hidden_canvas.toDataURL('image/png');
        }
    }


    function displayErrorMessage(error_msg, error){
        error = error || "";
        if(error){
            console.error(error);
        }

        error_message.innerText = error_msg;

        hideUI();
        error_message.classList.add("visible");
    }

   
    function hideUI(){
        // Helper function for clearing the app UI.

        controls.classList.remove("visible");
        start_camera.classList.remove("visible");
        video.classList.remove("visible");
        snap.classList.remove("visible");
        error_message.classList.remove("visible");
    }

});
