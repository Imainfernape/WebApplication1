document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("registerForm")?.addEventListener("submit", function (event) {
        if (!validateForm()) {
            event.preventDefault();
        }
    });

    document.getElementById("loginForm")?.addEventListener("submit", function (event) {
        if (!validateLogin()) {
            event.preventDefault();
        }
    });
});

function validateForm() {
    return validateInputs() && validateEmail() && validatePassword();
}

function validateLogin() {
    return validateEmail() && validatePassword();
}

function validateInputs() {
    let inputs = document.querySelectorAll(".validate-input");
    for (let input of inputs) {
        if (!input.value.trim()) {
            alert("All fields are required.");
            return false;
        }
    }
    return true;
}

function validateEmail() {
    let email = document.querySelector(".validate-email").value;
    let regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!regex.test(email)) {
        alert("Invalid email format.");
        return false;
    }
    return true;
}

function validatePassword() {
    let password = document.querySelector(".validate-password").value;
    if (password.length < 12) {
        alert("Password must be at least 12 characters.");
        return false;
    }
    return true;
}
