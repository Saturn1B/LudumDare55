const admin = require("firebase-admin");

const serviceAccount = require("./firebase-service-account.json");

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount)
});

const db = admin.firestore();

async function deleteOldSoftDeletedLevels() {
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() /*- 30*/);

    const snapshot = await db.collection("levels")
        .where("isDeleted", "==", true)
        .where("deletedAt", "<=", cutoff)
        .get();

    for (const doc of snapshot.docs) {
        console.log("Deleting level:", doc.id);
        await db.collection("levels").doc(doc.id).delete();
    }
}

async function deleteOrphanLikes() {
    const likesSnapshot = await db.collection("likes").get();

    for (const like of likesSnapshot.docs) {
        const levelId = like.data().levelId;

        const levelDoc = await db.collection("levels").doc(levelId).get();

        if (!levelDoc.exists) {
            console.log("Deleting orphan like:", like.id);
            await like.ref.delete();
        }
    }
}

async function recalculateCounters() {

    const usersSnapshot = await db.collection("users").get();

    for (const userDoc of usersSnapshot.docs) {

        const userId = userDoc.id;

        const likesSnapshot = await db.collection("likes")
            .where("userId", "==", userId)
            .get();

        const uploadedSnapshot = await db.collection("levels")
            .where("authorId", "==", userId)
            .where("isDeleted", "==", false)
            .get();

        await userDoc.ref.update({
            likedLevelsCount: likesSnapshot.size,
            uploadedLevelsCount: uploadedSnapshot.size
        });

        console.log("Updated counters for:", userId);
    }
}

async function runCleanup() {
    await deleteOldSoftDeletedLevels();
    await deleteOrphanLikes();
    await recalculateCounters();
    console.log("Cleanup completed.");
}

runCleanup();
