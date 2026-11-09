(function () {
    "use strict";

    const body = document.body;
    const menuBtn = document.getElementById("adminMenuBtn");
    const scrim = document.getElementById("adminScrim");
    const side = document.getElementById("adminSide");

    menuBtn?.addEventListener("click", () => body.classList.toggle("side-open"));
    scrim?.addEventListener("click", () => body.classList.remove("side-open"));
    side?.querySelectorAll("a").forEach((link) =>
        link.addEventListener("click", () => body.classList.remove("side-open"))
    );

    document.querySelectorAll("form[data-confirm]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (!window.confirm(form.dataset.confirm || "Are you sure?")) {
                event.preventDefault();
            }
        });
    });

    document.querySelectorAll("[data-file-input]").forEach((input) => {
        input.addEventListener("change", () => {
            const preview = document.querySelector(input.dataset.preview || "");
            if (!preview) return;
            preview.innerHTML = "";
            Array.from(input.files || []).forEach((file) => {
                if (!file.type.startsWith("image/")) return;
                const item = document.createElement("div");
                item.className = "preview-item";
                const img = document.createElement("img");
                img.src = URL.createObjectURL(file);
                img.alt = file.name;
                item.appendChild(img);
                preview.appendChild(item);
            });
        });
    });

    document.querySelectorAll(".preview-item.removable input[type=checkbox]").forEach((box) => {
        box.addEventListener("change", () => {
            box.closest(".preview-item")?.classList.toggle("remove-selected", box.checked);
        });
    });
})();
