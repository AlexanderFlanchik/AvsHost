<template>
    <div :style="wrapperStyle">
        <svg :viewBox="tagViewBox" xmlns="http://www.w3.org/2000/svg">
            <polygon :points="points" :fill="tagData.backgroundColor" :stroke="tagData.backgroundColor"></polygon>
            <circle :cx="side" :cy="side" :r="circleRadius" fill="white" stroke="navy"></circle>
            <text :x="x" :y="y" :style="textStyle">{{tagData.name}}</text>
        </svg>
    </div>
</template>
<script>
    const fontName = "18px serif";
    
    export default {
        data: function() {
            return {
                tagLength: 0,
                side: 20,
                circleRadius: 8,
                x: 0,
                y: 0
            };
        },

        props: {
            tagData: Object
        },

        mounted: function() {
            this.x = this.side + this.circleRadius + 5;
            this.y = this.side + this.circleRadius / 2;
            this.tagLength = this.getTagWidth(this.tagData.name, fontName);
        },
        methods: {
            getTagWidth: function(tagBody, font) {
                let canvas = document.createElement('canvas');
                let context = canvas.getContext('2d');
                context.font = font;
                let result = Math.floor(context.measureText(tagBody).width) + 2 * this.circleRadius + this.side;

                return result;
            }            
        },
        computed: {
            wrapperStyle: function() {
                return `width: ${this.tagLength}px;`;
            },
            tagViewBox: function() {
                return `0 0 ${this.tagLength} ${2 * this.side}`;
            },
            points: function() {
                return `0 ${this.side}, ${this.side} 0, ${this.tagLength} 0, ${this.tagLength} ${2 * this.side}, ${this.side} ${2 * this.side}`;
            },
            textStyle: function() {
                return {
                    font: fontName,
                    fill: this.tagData.textColor
                };
            }
        }
    }
</script>
<style scoped>

</style>