import { initializeApp } from "https://www.gstatic.com/firebasejs/11.0.1/firebase-app.js";
import {
    getAuth,
    signInWithEmailAndPassword,
    createUserWithEmailAndPassword,
    GoogleAuthProvider,
    signInWithPopup
} from "https://www.gstatic.com/firebasejs/11.0.1/firebase-auth.js";

const firebaseConfig = {
    apiKey: "YOUR_FIREBASE_WEB_API_KEY",
    authDomain: "YOUR_FIREBASE_PROJECT_ID.firebaseapp.com",
    projectId: "YOUR_FIREBASE_PROJECT_ID"
};

const app = initializeApp(firebaseConfig);
const auth = getAuth(app);
const googleProvider = new GoogleAuthProvider();

window.firebaseAuthInterop = {
    signInWithEmail: async (email, password) => {
        const cred = await signInWithEmailAndPassword(auth, email, password);
        return await cred.user.getIdToken();
    },
    signUpWithEmail: async (email, password) => {
        const cred = await createUserWithEmailAndPassword(auth, email, password);
        return await cred.user.getIdToken();
    },
    signInWithGoogle: async () => {
        const cred = await signInWithPopup(auth, googleProvider);
        return await cred.user.getIdToken();
    }
};