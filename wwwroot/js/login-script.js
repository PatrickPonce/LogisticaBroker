const signupButton = document.getElementById('signup-button');
const loginButton = document.getElementById('login-button');
const userForms = document.getElementById('user_options-forms');

signupButton.addEventListener('click', () => {
    userForms.classList.remove('bounceRight');
    userForms.classList.add('bounceLeft');
}, false);

loginButton.addEventListener('click', () => {
    userForms.classList.remove('bounceLeft');
    userForms.classList.add('bounceRight');
}, false);