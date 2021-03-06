
const video = document.getElementById('video')



Promise.all([

    faceapi.nets.tinyFaceDetector.loadFromUri('/models2'),

    faceapi.nets.faceLandmark68Net.loadFromUri('/models2'),

    faceapi.nets.faceRecognitionNet.loadFromUri('/models2'),

    faceapi.nets.faceExpressionNet.loadFromUri('/models2')

]).then(startVideo)



function startVideo() {

    var cameras = new Array(); //create empty array to later insert available devices
    navigator.mediaDevices.enumerateDevices() // get the available devices found in the machine
        .then(function (devices) {
            devices.forEach(function (device) {
                var i = 0;
                if (device.kind === "videoinput") { //filter video devices only
                    cameras[i] = device.deviceId; // save the camera id's in the camera array
                    i++;
                }
            });
        })
    

    navigator.getUserMedia(

        { video: {} },

        stream => video.srcObject = stream,

        err => console.error(err)

    )

}



video.addEventListener('play', () => {

    const canvas = faceapi.createCanvasFromMedia(video)

    document.body.append(canvas)

    const displaySize = { width: video.width, height: video.height }

    faceapi.matchDimensions(canvas, displaySize)

    setInterval(async () => {

        const detections = await faceapi.detectAllFaces(video, new faceapi.TinyFaceDetectorOptions()).withFaceLandmarks().withFaceExpressions()

        const resizedDetections = faceapi.resizeResults(detections, displaySize)

        canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height)

        faceapi.draw.drawDetections(canvas, resizedDetections)

        faceapi.draw.drawFaceLandmarks(canvas, resizedDetections)

        faceapi.draw.drawFaceExpressions(canvas, resizedDetections)

    }, 100)

})