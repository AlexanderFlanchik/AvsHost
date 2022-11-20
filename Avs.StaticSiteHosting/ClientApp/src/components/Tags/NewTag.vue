<template>
    <fieldset>
        <legend>Add New Tag</legend>
        <div>Tag name:</div>
        <div class="tag-input-container">
            <input type="text" v-model="name" />
        </div>
        <div>Background color:</div>
        <div class="tag-input-container">
            <input type="color" v-model="textColor" />
        </div>
        <div>Text color:</div>
        <div class="tag-input-container">
            <input type="color" v-model="backgroundColor" />
        </div>
        <div class="tag-input-container">
            <button class="btn btn-primary">Create</button>
        </div>
    </fieldset>
</template>
<script>
    export default {
        data: function() {
            return {
                name: '',
                textColor: '#FFFFFF',
                backgroundColor: '#000000'
            };
        }
    }
</script>
<style scoped>
 .tag-input-container {
    padding-bottom: 5px;
 }
</style>