document.getElementById("diamondButton").addEventListener("click", function () {
    let userQuestion = document.getElementById("userInput").value;

    fetch("/Home/AskAI", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Question: userQuestion })
    })
        .then(response => response.json())
        .then(data => {
            document.getElementById("aiResponse").innerText = data.answer;
        })
        .catch(error => console.error("Error:", error));
});
