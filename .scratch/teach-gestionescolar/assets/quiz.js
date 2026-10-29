/**
 * Quiz widget for teach lessons.
 * - Shuffles multiple-choice options at render (avoids position bias).
 * - Immediate feedback on click / check.
 * - Supports data-type="mc" | "tf" | "fill".
 *
 * Markup:
 * <div class="quiz" data-quiz>
 *   <div class="q" data-type="mc" data-answer="Dos tipos distintos; el repo traduce"
 *        data-explain="...">
 *     <p class="prompt">...</p>
 *     <button type="button" class="choice">...</button>
 *     ...
 *   </div>
 *   <div class="q" data-type="tf" data-answer="Falso" data-explain="...">
 *     <p class="prompt">...</p>
 *   </div>
 *   <div class="q" data-type="fill"
 *        data-answer="attendance_record|uq_attendance_record_session_student"
 *        data-explain="...">
 *     <p class="prompt">...</p>
 *   </div>
 * </div>
 */
(function () {
  function normalize(s) {
    return String(s || "")
      .trim()
      .toLowerCase()
      .normalize("NFD")
      .replace(/\p{M}/gu, "")
      .replace(/\s+/g, " ");
  }

  function shuffle(nodes) {
    const arr = Array.from(nodes);
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  function answersMatch(expectedRaw, givenRaw) {
    const expectedParts = String(expectedRaw)
      .split("|")
      .map(normalize)
      .filter(Boolean);
    const given = normalize(givenRaw);
    if (expectedParts.length === 1) return given === expectedParts[0];
    // Fill: accept if every expected token appears (order flexible for "a / b")
    const givenBits = given.split(/[\/,;]+|\s+y\s+/).map((x) => x.trim()).filter(Boolean);
    if (givenBits.length >= expectedParts.length) {
      return expectedParts.every((p) => givenBits.some((g) => g === p || g.includes(p) || p.includes(g)));
    }
    return expectedParts.every((p) => given.includes(p));
  }

  function setFeedback(q, ok, explain) {
    const el = q.querySelector(".feedback");
    if (!el) return;
    el.className = "feedback " + (ok ? "ok" : "bad");
    el.textContent = ok
      ? "Bien. " + (explain || "")
      : "No. " + (explain || "Probá de nuevo.");
  }

  function markAnswered(q, ok) {
    q.dataset.answered = "1";
    q.dataset.correct = ok ? "1" : "0";
    updateScore(q.closest("[data-quiz]"));
  }

  function updateScore(root) {
    if (!root) return;
    const bar = document.querySelector("[data-scorebar]");
    if (!bar) return;
    const qs = root.querySelectorAll(".q");
    const answered = root.querySelectorAll('.q[data-answered="1"]').length;
    const correct = root.querySelectorAll('.q[data-correct="1"]').length;
    bar.querySelector("[data-score-done]").textContent = String(answered);
    bar.querySelector("[data-score-total]").textContent = String(qs.length);
    bar.querySelector("[data-score-ok]").textContent = String(correct);
  }

  function wireMc(q) {
    const answer = q.getAttribute("data-answer");
    const explain = q.getAttribute("data-explain") || "";
    let choices = Array.from(q.querySelectorAll("button.choice"));
    if (!choices.length) return;

    const wrap = document.createElement("div");
    wrap.className = "choices";
    shuffle(choices).forEach((btn) => wrap.appendChild(btn));
    choices.forEach((btn) => btn.remove());
    const prompt = q.querySelector(".prompt");
    prompt.after(wrap);

    wrap.querySelectorAll("button.choice").forEach((btn) => {
      btn.addEventListener("click", () => {
        if (q.dataset.answered === "1") return;
        const ok = normalize(btn.textContent) === normalize(answer);
        wrap.querySelectorAll("button.choice").forEach((b) => {
          b.disabled = true;
          if (normalize(b.textContent) === normalize(answer)) b.classList.add("correct");
        });
        if (!ok) btn.classList.add("wrong");
        setFeedback(q, ok, explain);
        markAnswered(q, ok);
      });
    });
  }

  function wireTf(q) {
    const answer = q.getAttribute("data-answer");
    const explain = q.getAttribute("data-explain") || "";
    const wrap = document.createElement("div");
    wrap.className = "choices";
    ["Verdadero", "Falso"].forEach((label) => {
      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = "choice";
      btn.textContent = label;
      wrap.appendChild(btn);
    });
    const prompt = q.querySelector(".prompt");
    prompt.after(wrap);
    // Shuffle V/F too
    const shuffled = shuffle(wrap.querySelectorAll("button.choice"));
    shuffled.forEach((b) => wrap.appendChild(b));

    wrap.querySelectorAll("button.choice").forEach((btn) => {
      btn.addEventListener("click", () => {
        if (q.dataset.answered === "1") return;
        const ok = normalize(btn.textContent) === normalize(answer);
        wrap.querySelectorAll("button.choice").forEach((b) => {
          b.disabled = true;
          if (normalize(b.textContent) === normalize(answer)) b.classList.add("correct");
        });
        if (!ok) btn.classList.add("wrong");
        setFeedback(q, ok, explain);
        markAnswered(q, ok);
      });
    });
  }

  function wireFill(q) {
    const answer = q.getAttribute("data-answer");
    const explain = q.getAttribute("data-explain") || "";
    const row = document.createElement("div");
    row.className = "fill-row";
    const input = document.createElement("input");
    input.type = "text";
    input.className = "fill";
    input.setAttribute("aria-label", "Tu respuesta");
    input.placeholder = "Escribí la respuesta…";
    const check = document.createElement("button");
    check.type = "button";
    check.className = "check";
    check.textContent = "Chequear";
    row.appendChild(input);
    row.appendChild(check);
    q.querySelector(".prompt").after(row);

    function submit() {
      if (q.dataset.answered === "1") return;
      const ok = answersMatch(answer, input.value);
      input.disabled = true;
      check.disabled = true;
      setFeedback(q, ok, explain);
      markAnswered(q, ok);
    }
    check.addEventListener("click", submit);
    input.addEventListener("keydown", (e) => {
      if (e.key === "Enter") submit();
    });
  }

  function ensureFeedback(q) {
    if (!q.querySelector(".feedback")) {
      const fb = document.createElement("p");
      fb.className = "feedback";
      fb.setAttribute("aria-live", "polite");
      q.appendChild(fb);
    }
  }

  function initQuiz(root) {
    root.querySelectorAll(".q").forEach((q) => {
      ensureFeedback(q);
      const type = q.getAttribute("data-type") || "mc";
      if (type === "tf") wireTf(q);
      else if (type === "fill") wireFill(q);
      else wireMc(q);
    });
    updateScore(root);
  }

  function boot() {
    document.querySelectorAll("[data-quiz]").forEach(initQuiz);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
