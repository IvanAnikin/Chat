document.addEventListener('DOMContentLoaded', function () {

    // References to all the element we will need.
    var video = document.querySelector('#camera-stream'),
        image = document.querySelector('#snap'),
        start_camera = document.querySelector('#start-camera'),
        controls = document.querySelector('.controls'),
        take_photo_btn = document.querySelector('#take-photo'),
        delete_photo_btn = document.querySelector('#delete-photo'),
        download_photo_btn = document.querySelector('#download-photo'),
        error_message = document.querySelector('#error-message');
        send_btn = document.getElementById("sendPhotoBtn");

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

    send_btn.addEventListener("click", function (e) {

        const account = {
            name: "notesaccount"
        };
        var sas = document.getElementById('sasInput').value;

        const blobUri = 'https://' + account.name + '.blob.core.windows.net';

        var blobService = AzureStorage.Blob.createBlobServiceWithSas(blobUri, sas);

        var containerName = 'test-take-send';

        blobService.createContainerIfNotExists(containerName, (error, container) => {
            if (error) {
                // Handle create container error
            } else {
                console.log(container.name);
            }
        });

        const file = document.getElementById("imageIMG").src;
        file.name = "photo.png";

        blobService.createBlockBlobFromBrowserFile(containerName,
            'photo-test-name',
            file,
            (error, result) => {
                if (error) {
                    // Handle blob error
                } else {
                    console.log('Upload is successful');
                }
            });

    });


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

        alert(snap);

        snapp = snap;

        // Show image. 
        image.setAttribute('src', snap);
        image.classList.add("visible");

        // Enable delete and save buttons
        delete_photo_btn.classList.remove("disabled");
        download_photo_btn.classList.remove("disabled");

        // Set the href attribute of the download button to the snap url.
        download_photo_btn.href = snap;

        document.getElementById('results').innerHTML =
            '<h2>Here is your image:</h2>' +
            '<img id="imageIMG" src="' + snap + '"/>';

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
