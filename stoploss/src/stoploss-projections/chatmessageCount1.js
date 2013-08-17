fromAll()
.when({
    $init: function () {
        return { count: 0, }; // initial state
    },

    ChatMessage: function (s, e) {
        s.count += 1;
        return s;
    },
});