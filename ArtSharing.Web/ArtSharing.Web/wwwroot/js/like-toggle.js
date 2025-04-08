document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".like-form").forEach(form => {
        form.addEventListener("submit", async function (e) {
            e.preventDefault();

            const postId = this.dataset.postId;
            const token = this.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const response = await fetch('/Like/ToggleLike', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ postId })
            });

            if (response.ok) {
                const result = await response.json();

                const icon = this.querySelector(".heart-icon");
                if (icon) {
                    icon.textContent = result.hasLiked ? "❤️" : "🤍";
                    icon.className = result.hasLiked ? "heart-icon text-danger" : "heart-icon text-secondary";
                }

                const count = document.querySelector(`#like-count-${postId}`);
                if (count) {
                    count.textContent = `(${result.likeCount} like${result.likeCount === 1 ? "" : "s"})`;
                }

                const container = this.closest(".liked-post-card");
                if (container && !result.hasLiked && window.location.href.includes("LikedPosts")) {
                    container.remove();
                }
            }
        });
    });
});
