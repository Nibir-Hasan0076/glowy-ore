(function () {
    "use strict";

    const header = document.getElementById("siteHeader");
    const navToggle = document.getElementById("navToggle");
    const navScrim = document.getElementById("navScrim");

    /* sticky header shadow */
    const onScroll = () => {
        if (!header) return;
        header.classList.toggle("scrolled", window.scrollY > 10);
    };
    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();

    /* mobile navigation */
    const closeNav = () => {
        if (!header) return;
        header.classList.remove("nav-open");
        navToggle?.setAttribute("aria-expanded", "false");
        document.body.style.overflow = "";
    };
    navToggle?.addEventListener("click", () => {
        const open = header.classList.toggle("nav-open");
        navToggle.setAttribute("aria-expanded", String(open));
        document.body.style.overflow = open ? "hidden" : "";
    });
    navScrim?.addEventListener("click", closeNav);

    document.querySelectorAll(".main-nav a").forEach((link) => {
        link.addEventListener("click", () => {
            if (window.innerWidth <= 900) closeNav();
        });
    });

    const dropBtn = document.querySelector(".nav-drop-btn");
    dropBtn?.addEventListener("click", (event) => {
        event.preventDefault();
        dropBtn.closest(".nav-drop")?.classList.toggle("open");
    });

    /* toast */
    let toastTimer;
    window.showToast = (message) => {
        let toast = document.getElementById("toast");
        if (!toast) {
            toast = document.createElement("div");
            toast.id = "toast";
            toast.className = "toast";
            toast.setAttribute("role", "status");
            toast.innerHTML = "<span></span>";
            document.body.appendChild(toast);
        }
        toast.querySelector("span").textContent = message;
        requestAnimationFrame(() => toast.classList.add("show"));
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => toast.classList.remove("show"), 3200);
    };

    const toast = document.getElementById("toast");
    if (toast) {
        requestAnimationFrame(() => toast.classList.add("show"));
        toastTimer = setTimeout(() => toast.classList.remove("show"), 3600);
    }

    /* scroll reveal */
    const revealItems = document.querySelectorAll("[data-reveal]");
    if ("IntersectionObserver" in window) {
        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add("is-visible");
                        observer.unobserve(entry.target);
                    }
                });
            },
            { threshold: 0.12, rootMargin: "0px 0px -40px 0px" }
        );
        revealItems.forEach((el) => observer.observe(el));
    } else {
        revealItems.forEach((el) => el.classList.add("is-visible"));
    }

    /* quick add to bag without leaving the page */
    document.querySelectorAll("form[data-quick-add]").forEach((form) => {
        form.addEventListener("submit", async (event) => {
            event.preventDefault();
            const button = form.querySelector("button");
            if (button) button.disabled = true;
            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form),
                    credentials: "same-origin"
                });
                if (!response.ok) throw new Error("request failed");
                const badge = document.getElementById("cartCount");
                if (badge) {
                    const next = (parseInt(badge.textContent, 10) || 0) + 1;
                    badge.textContent = String(next);
                    badge.classList.remove("is-empty");
                }
                window.showToast("Added to your bag ✨");
            } catch {
                form.submit();
            } finally {
                if (button) button.disabled = false;
            }
        });
    });

    /* quantity steppers */
    document.querySelectorAll("[data-stepper]").forEach((stepper) => {
        const input = stepper.querySelector("input");
        const dec = stepper.querySelector("[data-dec]");
        const inc = stepper.querySelector("[data-inc]");
        const max = parseInt(stepper.dataset.max || "99", 10);
        const sync = (value) => {
            const next = Math.min(max, Math.max(1, value));
            if (input) input.value = String(next);
        };
        dec?.addEventListener("click", () => sync((parseInt(input?.value, 10) || 1) - 1));
        inc?.addEventListener("click", () => sync((parseInt(input?.value, 10) || 1) + 1));
    });

    /* product gallery */
    const galleryMain = document.getElementById("galleryMain");
    document.querySelectorAll("[data-thumb]").forEach((thumb) => {
        thumb.addEventListener("click", () => {
            if (!galleryMain) return;
            const src = thumb.dataset.src;
            if (!src) return;
            galleryMain.classList.add("fade");
            setTimeout(() => {
                galleryMain.src = src;
                galleryMain.classList.remove("fade");
            }, 180);
            document.querySelectorAll("[data-thumb]").forEach((t) => t.classList.remove("active"));
            thumb.classList.add("active");
        });
    });

    /* newsletter forms */
    document.querySelectorAll("form[data-newsletter]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            event.preventDefault();
            const input = form.querySelector("input");
            if (input && input.value.trim()) {
                window.showToast("You're on the glow list ✨");
                input.value = "";
            }
        });
    });

    /* graceful fallback for missing images */
    document.addEventListener(
        "error",
        (event) => {
            const target = event.target;
            if (target instanceof HTMLImageElement && !target.dataset.fallback) {
                target.dataset.fallback = "1";
                target.src = "/images/placeholder.svg";
                target.classList.add("img-missing");
            }
        },
        true
    );
})();
