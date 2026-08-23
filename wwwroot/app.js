const $ = id => document.getElementById(id);
let mode = 'login';
const setMessage = (text = '', type = '') => { $('message').textContent = text; $('message').className = type; };

function setMode(next) {
  mode = next;
  $('loginTab').classList.toggle('active', mode === 'login');
  $('registerTab').classList.toggle('active', mode === 'register');
  $('submitButton').textContent = mode === 'login' ? 'Sign in' : 'Create account';
  setMessage();
}
$('loginTab').onclick = () => setMode('login');
$('registerTab').onclick = () => setMode('register');

$('authForm').onsubmit = async event => {
  event.preventDefault();
  setMessage();
  const body = { email: $('email').value, password: $('password').value };
  try {
    const response = await fetch(`/api/auth/${mode}`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
    const data = await response.json();
    if (!response.ok) throw new Error(data.error || 'Something went wrong.');
    if (mode === 'register') {
      setMode('login');
      setMessage(data.message, 'success');
      $('password').value = '';
    } else {
      showFactPanel(body.email, data.fact);
    }
  } catch (error) { setMessage(error.message, 'error'); }
};

function showFactPanel(email, fact) {
  $('authPanel').classList.add('hidden');
  $('factPanel').classList.remove('hidden');
  $('welcome').textContent = `Signed in as ${email}`;
  $('fact').textContent = fact;
  setMessage();
}

$('newFact').onclick = async () => {
  const response = await fetch('/api/facts/random');
  if (response.status === 401) return location.reload();
  const data = await response.json();
  $('fact').textContent = data.fact;
};

$('logout').onclick = async () => {
  await fetch('/api/auth/logout', { method: 'POST' });
  location.reload();
};

(async function restoreSession() {
  const response = await fetch('/api/me');
  if (response.ok) {
    const me = await response.json();
    const factResponse = await fetch('/api/facts/random');
    const data = await factResponse.json();
    showFactPanel(me.email, data.fact);
  }
})();
