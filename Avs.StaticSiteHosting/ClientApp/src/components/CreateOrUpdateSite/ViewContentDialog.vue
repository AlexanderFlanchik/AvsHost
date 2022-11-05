<template>
    <b-modal ref="view-content-dlg" hide-footer size="xl" :title="fileName">
        <div class="content-centered">
            <img :src="src" />
        </div>
        <div class="content-centered dlg-btn-container">
            <button class="btn btn-primary" @click="closeViewContent">Close</button>
        </div>
    </b-modal>
</template>
<script>
    export default {
        data: function() {
            return {
                fileName: '',
                src: ''
            };
        },

        methods: {
            open: function(fileName, src) {
                this.fileName = fileName;
                this.src = src;
                this.$refs["view-content-dlg"].show();
            },

            closeViewContent: function() {
                this.$refs["view-content-dlg"].hide();
            }
        }
    }
</script>