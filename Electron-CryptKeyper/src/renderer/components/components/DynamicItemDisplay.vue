<template>
    <div>
        <div class="content flex-container" :class="{ 'open': isSidebarOpen}">
            <div class="column" v-for="(field, count) in fieldNames" :key="field">
                <h2>{{ field }}</h2> 
                <section class="item-holder" v-for="(item, index) in filteredFields[count]" :key="item.id">
                    <p :key="item.id" :class="{ 'hidden-text': isFieldHidden[count][index]}" @click="copyField(filteredFields[count][index])">{{ filteredFields[count][index] }} </p>
                    <button v-if="hiddenFields[count]" class="show-button" @click="showField(count,index)">👁</button>
                </section>
            </div>

            <!-- <div class="final column">
                <h2></h2>
                <section class="item-holder" v-for="(item, index) in filteredFields[0]" :key="item.id">
                    <button class="delete-button" @click="deleteItem(filteredFields[0][0])">🗑️</button>
                </section>
            </div> -->
        </div>
    </div>

</template>

<script>
    export default {
        name: 'DynamicItemDisplay',
        props: ['isSidebarOpen', 'fieldNames', 'filteredFields', 'hiddenFields'],
        data() {
            return {
                isFieldHidden: [], // Array of arrays of booleans indicating whether or not a field is hidden
            };
        },
        computed: {
        },
        created() {
            this.setupHiddenFields();
        },

        watch: {
            filteredFields() {
                this.setupHiddenFields();
            }
        },
        
        methods: {
            deleteItem(id) {
                console.log(this.filteredFields)
            },

            showField(count, index) {
                this.$set(this.isFieldHidden[count], index, !this.isFieldHidden[count][index]);
            },

            setupHiddenFields() {
                let tempHidden = [];
                for(let i = 0; i < this.fieldNames.length; i++) {
                    if (this.hiddenFields[i]) {
                        let temp = [];
                        for(let j = 0; j < this.filteredFields[i].length; j++) {
                            temp.push(true);
                        }
                        tempHidden.push(temp);
                    } else {
                        tempHidden.push(Array(this.filteredFields[i].length).fill(false));
                    }
                }
                this.$set(this, 'isFieldHidden', tempHidden);
            },
            
            copyField(text) {
                try {
                    if(!navigator.clipboard) throw('Unsupported browser');
                    navigator.clipboard.writeText(text);
                } catch(error) {
                    // Fallback for insecure browsers / Localhost testing
                    const textArea = document.createElement('textarea');
                    textArea.value = text;
                    document.body.appendChild(textArea);
                    textArea.focus();
                    textArea.select();
                    document.execCommand('copy');
                    document.body.removeChild(textArea);
                }

                this.$emit('copied')
            },
        }
    }
</script>

<!-- Style could use some cleanup, and items in App.vue can be moved here. -->

<style>
.flex-container {
    display: flex;
    flex-wrap: nowrap;
    justify-content: space-around;
    align-items: center;
    align-content: center;
}

.decrypted-item {
    display: flex;
    flex-wrap: nowrap;
    flex-direction: row;
    justify-content: space-around;
    align-items: center;
    align-content: center;
    border: 1px solid black;
    width: 100%;
    border-radius: 5px;
    padding: 5px;
    margin: 5px;
}

.item-holder {
    display: flex;
    flex-wrap: nowrap;
    flex-direction: row;
    justify-content: space-around;
    align-items: center;
    align-content: center;
    width: 100%;
}

.item-field {
    width: 100%;
    white-space: nowrap;
    border-radius: 8px;
    font-weight: bold;
    font-size: large;
    padding: 5px;
    background-color: #c1c1c1;
    text-align: center;
}

.delete-button {
    margin-left: auto;
    margin-right: 20px;
    border: 2px solid lightgrey;
    background-color: darkslategrey;
    position: relative;
    color: white;
}

.show-button {
    margin-left: auto;
    margin-right: 20px;
    border: 2px solid transparent;
    background-color: transparent;
    position: relative;
    font-size: xx-large;
    color: grey;
}

.hidden-text {
    -webkit-text-security: disc;
}

.delete-button:hover {
    background-color: crimson;
    color: black;
}
</style>