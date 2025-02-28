
<script setup lang="ts">
    import { onMounted, ref, watch } from "vue";

    const props = defineProps<{ modelValue: string | undefined }>();
    const formattedValue = ref("");

    watch(() => props.modelValue, (newValue) => {
        formattedValue.value = newValue ?? "";
        formatValue();
    });

    onMounted(() => {
        if (!formattedValue.value) {
            formattedValue.value = props.modelValue ?? '';
            formatValue();
        }
    });

    const formatValue = (isDeleting: boolean = false) => {
        formattedValue.value = formattedValue.value.replace(/[^0-9:]/g, '');
        if (!formattedValue.value.length) {
            return;
        }

        if (formattedValue.value.length == 2) {
            formattedValue.value += ":";
        }

        const last = formattedValue.value[formattedValue.value.length - 1];
        if (formattedValue.value.length == 3 && last !== ':') {
            formattedValue.value = `${formattedValue.value.substring(0, formattedValue.value.length - 1)}:${last}`;
        }    

        if (formattedValue.value.length <= 3 && isDeleting) {
            formattedValue.value = formattedValue.value.replace(':', '');
        }

        if (formattedValue.value.length > 5) {
            formattedValue.value = formattedValue.value.substring(0, 5);
        }
    };

    const emit = defineEmits(['update:modelValue']);
    const formatInput = (isDeleting: boolean) => {
        if (!formattedValue.value) {
            return;
        }

        formatValue(isDeleting);
    };

    const onInputChange = (event : any) => {
        formattedValue.value = event.target.value;
        const isDeleting = event.inputType === "deleteContentBackward";
        formatInput(isDeleting);
        
        // Emit the updated value to the parent using v-model
        emit('update:modelValue', formattedValue.value);
    }; 

    const onInputBlur = () => {
        if (!formattedValue.value) {
            return;
        }
        if (formattedValue.value.length === 1) {
            formattedValue.value = `00:0${formattedValue.value}`;
        } else {
            let [hh, mm] = formattedValue.value.split(':');
            if (hh.length < 2) {
                hh += '0';
            }

            if (!mm.length) {
                mm = '00';
            }
            else if (mm.length === 1) {
                mm += '0';
            } else {
                if (Number(mm) >= 60) {
                    const minutes = Number(mm);
                    const hours = Number(hh);
                    
                    const timeToMinutes = minutes  - 60;
                    const timeToHours = hours + 1;

                    mm = timeToMinutes >= 10 ? timeToMinutes.toString() : '0' + timeToMinutes.toString();
                    hh = timeToHours >= 10 ? timeToHours.toFixed(0) : '0' + timeToHours.toFixed(0);
                }
            }

            formattedValue.value = `${hh}:${mm}`;
        }
        
        // Emit the updated value to the parent using v-model
        emit('update:modelValue', formattedValue.value);
    };
</script>
<template>
    <input
      v-model="formattedValue"
      @input="onInputChange"
      @blur="onInputBlur"
      type="text"
      placeholder="hh:mm"
      maxlength="5"
    />
</template>
<style scoped>
   input {
    width: 45px;
   }
</style>