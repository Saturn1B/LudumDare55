const cloudinary = require("cloudinary").v2;
const admin = require("firebase-admin");

// ===== CONFIG (from environment - never hardcode these) =====
const CLOUD_NAME = process.env.CLOUDINARY_CLOUD_NAME;
const API_KEY = process.env.CLOUDINARY_API_KEY;
const API_SECRET = process.env.CLOUDINARY_API_SECRET;
const CLOUDINARY_FOLDER = "level_thumbnails";

// ===== INIT CLOUDINARY =====
cloudinary.config({
  cloud_name: CLOUD_NAME,
  api_key: API_KEY,
  api_secret: API_SECRET
});

// ===== INIT FIREBASE ADMIN =====
admin.initializeApp({
  credential: admin.credential.cert(
    require("./firebase-service-account.json")
  )
});

const db = admin.firestore();

// ===== FETCH USED THUMBNAILS FROM FIRESTORE =====
async function getUsedThumbnailPublicIds() {
  const snapshot = await db.collection("levels").get();
  const used = new Set();

  snapshot.forEach(doc => {
    const data = doc.data();
    if (data.thumbnailPublicId) {
      used.add(data.thumbnailPublicId);
    }
  });

  return used;
}

// ===== FETCH ALL CLOUDINARY ASSETS =====
async function getAllCloudinaryAssets() {
  let results = [];
  let nextCursor = null;

  do {
    const response = await cloudinary.api.resources({
      type: "upload",
      prefix: CLOUDINARY_FOLDER,
      max_results: 500,
      next_cursor: nextCursor
    });

    results = results.concat(response.resources);
    nextCursor = response.next_cursor;
  } while (nextCursor);

  return results;
}

// ===== MAIN CLEANUP =====
async function cleanup() {
  console.log("Fetching Firestore thumbnails...");
  const usedPublicIds = await getUsedThumbnailPublicIds();

  console.log(`Used thumbnails: ${usedPublicIds.size}`);

  console.log("Fetching Cloudinary assets...");
  const assets = await getAllCloudinaryAssets();

  console.log(`Cloudinary assets found: ${assets.length}`);

  let deletedCount = 0;

  for (const asset of assets) {
    if (!usedPublicIds.has(asset.public_id)) {
      console.log(`Deleting unused: ${asset.public_id}`);
      await cloudinary.uploader.destroy(asset.public_id);
      deletedCount++;
    }
  }

  console.log(`Cleanup complete. Deleted ${deletedCount} images.`);
}

cleanup()
  .then(() => process.exit(0))
  .catch(err => {
    console.error(err);
    process.exit(1);
  });